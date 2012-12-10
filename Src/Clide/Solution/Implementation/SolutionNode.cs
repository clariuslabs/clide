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
    using System.IO;
    using System.Linq;
    using Clide.Events;
    using System.Dynamic;
    using Clide.Patterns.Adapter;
    using Clide.Properties;
    using Clide.VisualStudio;

    internal class SolutionNode : SolutionTreeNode, ISolutionNode
	{
		private Lazy<EnvDTE.Solution> solution;
		private ISolutionEvents events;

		public SolutionNode(
			SolutionNodeKind nodeKind,
			IVsSolutionHierarchyNode hierarchyNode,
			ITreeNodeFactory<IVsSolutionHierarchyNode> nodeFactory,
			IAdapterService adapter,
			ISolutionEvents solutionEvents)
			: base(nodeKind, hierarchyNode, new Lazy<ITreeNode>(() => null), nodeFactory, adapter)
		{
			this.solution = new Lazy<EnvDTE.Solution>(() => hierarchyNode.ServiceProvider.GetService<EnvDTE.DTE>().Solution);
			this.events = solutionEvents;
		}

		public dynamic Data
		{
			// TODO: implement
			get { return new ExpandoObject(); }
		}

		public bool IsOpen
		{
			get { return this.solution.Value.IsOpen; }
		}

		public void Open(string solutionFile)
		{
			Guard.NotNullOrEmpty(() => solutionFile, solutionFile);

			this.solution.Value.Open(solutionFile);
		}

		public void Create(string solutionFile)
		{
			Guard.NotNullOrEmpty(() => solutionFile, solutionFile);

			Guard.IsValid(
				() => solutionFile, solutionFile,
				s => Path.IsPathRooted(s),
				Strings.SolutionNode.InvalidSolutionFile);

			((EnvDTE80.Solution2)this.solution.Value).Create(Path.GetDirectoryName(solutionFile), Path.GetFileNameWithoutExtension(solutionFile));
		}

		public void Close(bool saveFirst = true)
		{
			this.solution.Value.Close(saveFirst);
		}

		public ISolutionFolderNode CreateSolutionFolder(string name)
		{
			Guard.NotNullOrEmpty(() => name, name);

			((EnvDTE80.Solution2)this.solution.Value).AddSolutionFolder(name);

			var solutionfolder =
				this.HierarchyNode.Children.Single(child => child.VsHierarchy.Properties(child.ItemId).DisplayName == name);

			return this.CreateNode(solutionfolder) as ISolutionFolderNode;
		}

		event EventHandler ISolutionEvents.SolutionOpened
		{
			add { this.events.SolutionOpened += value; }
			remove { this.events.SolutionOpened -= value; }
		}

		event EventHandler ISolutionEvents.SolutionClosing
		{
			add { this.events.SolutionClosing += value; }
			remove { this.events.SolutionClosing -= value; }
		}

		event EventHandler ISolutionEvents.SolutionClosed
		{
			add { this.events.SolutionClosed += value; }
			remove { this.events.SolutionClosed -= value; }
		}

		event EventHandler<ProjectEventArgs> ISolutionEvents.ProjectOpened
		{
			add { this.events.ProjectOpened += value; }
			remove { this.events.ProjectOpened -= value; }
		}

		event EventHandler<ProjectEventArgs> ISolutionEvents.ProjectClosing
		{
			add { this.events.ProjectClosing += value; }
			remove { this.events.ProjectClosing -= value; }
		}

	}
}