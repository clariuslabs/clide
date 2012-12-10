#region BSD License
/* 
Copyright (c) 2012, Clarius Consulting
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

namespace Clide.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using EnvDTE80;
    using EnvDTE;
    using Clide.Solution;

    [Export(typeof(IGlobalEvents))]
	[Export(typeof(ISolutionEvents))]
	[PartCreationPolicy(CreationPolicy.Shared)]
	internal class SolutionEvents : IDisposable, IVsSolutionEvents, ISolutionEvents
	{
		private bool isDisposed;
		private uint solutionEventsCookie;
		private IVsSolution solution;
		private Lazy<ITreeNodeFactory<IVsSolutionHierarchyNode>> nodeFactory;

		public event EventHandler SolutionOpened = (sender, args) => { };
		public event EventHandler SolutionClosed = (sender, args) => { };
		public event EventHandler SolutionClosing = (sender, args) => { };
		// We don't pre-initialize this event so as not to even check for node factory compatibility 
		// before raising the event. This would prevent slowing down VS when nobody's listening.
		public event EventHandler<ProjectEventArgs> ProjectOpened;
		public event EventHandler<ProjectEventArgs> ProjectClosing;

		[ImportingConstructor]
		public SolutionEvents(
			[Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
			[Import(DefaultHierarchyFactory.ContractName)] Lazy<ITreeNodeFactory<IVsSolutionHierarchyNode>> nodeFactory)
		{
			Guard.NotNull(() => serviceProvider, serviceProvider);
			Guard.NotNull(() => nodeFactory, nodeFactory);

			this.solution = serviceProvider.GetService<SVsSolution, IVsSolution>();
			this.nodeFactory = nodeFactory;

			ErrorHandler.ThrowOnFailure(
				this.solution.AdviseSolutionEvents(this, out this.solutionEventsCookie));
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!this.isDisposed)
			{
				if (disposing && (this.solutionEventsCookie != 0))
				{
					try
					{
						ErrorHandler.ThrowOnFailure(this.solution.UnadviseSolutionEvents(this.solutionEventsCookie));
						this.solutionEventsCookie = 0;
					}
					catch (Exception ex)
					{
						if (ErrorHandler.IsCriticalException(ex))
						{
							throw;
						}
					}
				}

				this.isDisposed = true;
			}
		}

		private ITreeNode GetParentNode(IVsHierarchy hierarchy)
		{
			if (hierarchy is IVsSolution)
				return null;

			object parent;
			ErrorHandler.ThrowOnFailure(
				hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_Parent, out parent));

			var parentHierarchy = parent as IVsHierarchy;
			var parentNode = new VsSolutionHierarchyNode(parentHierarchy);

			if (this.nodeFactory.Value.Supports(parentNode))
				return GetNode(parentHierarchy, parentNode);

			return null;
		}

		private ITreeNode GetNode(IVsHierarchy hierarchy, IVsSolutionHierarchyNode node)
		{
			return this.nodeFactory.Value.CreateNode(new Lazy<ITreeNode>(() => GetParentNode(hierarchy)), node);
		}

		int IVsSolutionEvents.OnBeforeCloseSolution(object pUnkReserved)
		{
			this.SolutionClosing(this, EventArgs.Empty);

			return VSConstants.S_OK;
		}

		int IVsSolutionEvents.OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
		{
			this.SolutionOpened(this, EventArgs.Empty);

			return VSConstants.S_OK;
		}

		int IVsSolutionEvents.OnAfterCloseSolution(object pUnkReserved)
		{
			this.SolutionClosed(this, EventArgs.Empty);

			return VSConstants.S_OK;
		}

		int IVsSolutionEvents.OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
		{
			// Quickly exit if there are no subscribers.
			if (this.ProjectOpened == null)
				return VSConstants.S_OK;

			object extObject;
			ErrorHandler.ThrowOnFailure(
				pHierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out extObject));

			var project = extObject as Project;

			// This event is also fired when a solution folder is added/loaded
			if (project != null && !(project.Object is SolutionFolder))
			{
				var node = new VsSolutionHierarchyNode(pHierarchy);
				if (this.nodeFactory.Value.Supports(node))
				{
					this.ProjectOpened(this,
						new ProjectEventArgs(new Lazy<IProjectNode>(() =>
							GetNode(pHierarchy, node).As<IProjectNode>())));
				}
			}

			return VSConstants.S_OK;
		}

		int IVsSolutionEvents.OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
		{
			// Quickly exit if there are no subscribers.
			if (this.ProjectClosing == null)
				return VSConstants.S_OK;

			object extObject;
			ErrorHandler.ThrowOnFailure(
				pHierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out extObject));

			var project = extObject as Project;

			//This event is also fired when a solution folder is added/loaded
			if (project != null && !(project.Object is SolutionFolder))
			{
				var node = new VsSolutionHierarchyNode(pHierarchy);
				if (this.nodeFactory.Value.Supports(node))
				{
					this.ProjectClosing(this,
						new ProjectEventArgs(new Lazy<IProjectNode>(() =>
							GetNode(pHierarchy, node).As<IProjectNode>())));
				}
			}

			return VSConstants.S_OK;
		}

		int IVsSolutionEvents.OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
		{
			return VSConstants.S_OK;
		}

		int IVsSolutionEvents.OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
		{
			return VSConstants.S_OK;
		}

		int IVsSolutionEvents.OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
		{
			return VSConstants.S_OK;
		}

		int IVsSolutionEvents.OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
		{
			return VSConstants.S_OK;
		}

		int IVsSolutionEvents.OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
		{
			return VSConstants.S_OK;
		}
	}
}