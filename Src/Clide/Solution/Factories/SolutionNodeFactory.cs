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
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using Clide.Events;
    using Clide.Patterns.Adapter;
    using Microsoft.VisualStudio.Shell;
    using Clide.Composition;

    [FallbackFactory]
	internal class SolutionNodeFactory : ITreeNodeFactory<IVsSolutionHierarchyNode>
	{
		private Lazy<ITreeNodeFactory<IVsSolutionHierarchyNode>> nodeFactory;
		private ISolutionEvents solutionEvents;
		private IAdapterService adapter;
        private IServiceProvider serviceProvider;
        private ISolutionExplorerNodeFactory explorerNodeFactory;

		public SolutionNodeFactory(
            IServiceProvider serviceProvider,
			[WithKey(DefaultHierarchyFactory.RegisterKey)] Lazy<ITreeNodeFactory<IVsSolutionHierarchyNode>> nodeFactory,
            ISolutionExplorerNodeFactory explorerNodeFactory,
			ISolutionEvents solutionEvents,
			IAdapterService adapter)
		{
            this.serviceProvider = serviceProvider;
			this.nodeFactory = nodeFactory;
            this.explorerNodeFactory = explorerNodeFactory;
			this.solutionEvents = solutionEvents;
			this.adapter = adapter;
		}

		public bool Supports(IVsSolutionHierarchyNode hierarchy)
		{
			return hierarchy.VsHierarchy is IVsSolution;
		}

		public ITreeNode CreateNode(Lazy<ITreeNode> parent, IVsSolutionHierarchyNode hierarchy)
		{
			return Supports(hierarchy) ?
				new SolutionNode(hierarchy, this.nodeFactory.Value, this.explorerNodeFactory, this.serviceProvider,  this.adapter, this.solutionEvents) : null;
		}
	}
}