#region BSD License
/* 
Copyright (c) 2012, Clarius Consulting
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

namespace Clide.Solution
{
    using System;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Design;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using System.Collections.Generic;
    using System.Linq;
    using Clide.VisualStudio;

    [Export(typeof(IToolWindow))]
	[Export(typeof(ISolutionExplorer))]
	internal class SolutionExplorer : ISolutionExplorer
	{
		private Lazy<ISolutionNode> solution;
        private VsToolWindow toolWindow;
        private IVsMonitorSelection selection;
        private Lazy<ITreeNodeFactory<IVsSolutionHierarchyNode>> nodeFactory;

		[ImportingConstructor]
		public SolutionExplorer(
			[Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
            [ImportMany(CompositionTarget.SolutionExplorer)] 
            IEnumerable<Lazy<ITreeNodeFactory<IVsSolutionHierarchyNode>, ITreeNodeFactoryMetadata>> nodeFactories)
		{
			Guard.NotNull(() => serviceProvider, serviceProvider);
            Guard.NotNull(() => nodeFactories, nodeFactories);

            this.toolWindow = serviceProvider.ToolWindow(StandardToolWindows.ProjectExplorer);
            this.selection = serviceProvider.GetService<SVsShellMonitorSelection, IVsMonitorSelection>();
            this.nodeFactory = new Lazy<ITreeNodeFactory<IVsSolutionHierarchyNode>>(() =>
                new FallbackNodeFactory<IVsSolutionHierarchyNode>(
                    new AggregateNodeFactory<IVsSolutionHierarchyNode>(nodeFactories.Where(n => !n.Metadata.IsFallback).Select(f => f.Value)),
                    new AggregateNodeFactory<IVsSolutionHierarchyNode>(nodeFactories.Where(n => n.Metadata.IsFallback).Select(f => f.Value))));

            this.solution = new Lazy<ISolutionNode>(() =>
                this.nodeFactory.Value.CreateNode(null, 
                    new VsSolutionHierarchyNode(serviceProvider.GetService<IVsSolution>() as IVsHierarchy, VSConstants.VSITEMID_ROOT))
                    .As<ISolutionNode>());
		}

		public ISolutionNode Solution { get { return this.solution.Value; } }

		public bool IsVisible
		{
            get { return this.toolWindow.IsVisible; }
		}

		public void Show()
		{
            this.toolWindow.Show();
		}

		public void Close()
		{
            this.toolWindow.Close();
		}

        public IEnumerable<ISolutionExplorerNode> SelectedNodes
        {
            get 
            {
                Func<IVsSolutionHierarchyNode, Lazy<ITreeNode>> getParent = null;
                Func<IVsSolutionHierarchyNode, ITreeNode> getNode = null;

                getNode = hierarchy => hierarchy == null ? null :
                    this.nodeFactory.Value.CreateNode(getParent(hierarchy), hierarchy);

                getParent = hierarchy => hierarchy.Parent == null ? null :
                    new Lazy<ITreeNode>(() => this.nodeFactory.Value.CreateNode(getParent(hierarchy.Parent), hierarchy.Parent));

                return this.selection.GetSelection()
                    .Select(sel => getNode(new VsSolutionHierarchyNode(sel.Item1, sel.Item2)))
                    .OfType<ISolutionExplorerNode>();
            }
        }
    }
}
