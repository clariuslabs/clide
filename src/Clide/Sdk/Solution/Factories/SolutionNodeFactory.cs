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

namespace Clide.Sdk.Solution
{
    using Clide.CommonComposition;
    using Clide.Events;
    using Clide.Patterns.Adapter;
    using Clide.Solution.Implementation;
    using Microsoft.Practices.ServiceLocation;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;

    /// <summary>
    /// Factory for <see cref="Clide.Solution.ISolutionNode"/>.
    /// </summary>
    [Named("SolutionExplorer")]
    [FallbackFactory]
	public class SolutionNodeFactory : ITreeNodeFactory<IVsSolutionHierarchyNode>
	{
		private Lazy<ITreeNodeFactory<IVsSolutionHierarchyNode>> childNodeFactory;
		private ISolutionEvents solutionEvents;
		private IAdapterService adapter;
        private IServiceLocator locator;
        private ISolutionExplorerNodeFactory looseNodeFactory;
        private IUIThread uiThread;

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionNodeFactory"/> class.
        /// </summary>
        /// <param name="serviceLocator">The service locator.</param>
        /// <param name="childNodeFactory">The factory for nodes, used to construct child nodes automatically.</param>
        /// <param name="looseNodeFactory">The explorer node factory used to create "loose" nodes from solution explorer.</param>
        /// <param name="solutionEvents">The solution events.</param>
        /// <param name="adapter">The adapter service that implements the smart cast <see cref="ITreeNode.As{T}"/>.</param>
        /// <param name="uiThread">The UI thread.</param>
		public SolutionNodeFactory(
            IServiceLocator serviceLocator,
			[Named(DefaultHierarchyFactory.RegisterKey)] Lazy<ITreeNodeFactory<IVsSolutionHierarchyNode>> childNodeFactory,
            ISolutionExplorerNodeFactory looseNodeFactory,
			ISolutionEvents solutionEvents,
			IAdapterService adapter, 
            IUIThread uiThread)
		{
            this.locator = serviceLocator;
			this.childNodeFactory = childNodeFactory;
            this.looseNodeFactory = looseNodeFactory;
			this.solutionEvents = solutionEvents;
			this.adapter = adapter;
            this.uiThread = uiThread;
		}

        /// <summary>
        /// Determines whether this factory supports the given hierarchy node.
        /// </summary>
        /// <param name="hierarchy">The hierarchy node to check.</param>
        /// <returns><see langword="true"/> if the given node is a solution supported by this factory; <see langword="false"/> otherwise.</returns>
        public virtual bool Supports(IVsSolutionHierarchyNode hierarchy)
		{
			return hierarchy.VsHierarchy is IVsSolution;
		}

        /// <summary>
        /// Creates the folder node if supported by this factory.
        /// </summary>
        /// <param name="parent">The accessor for the parent node of this folder.</param>
        /// <param name="hierarchy">The hierarchy node to construct the folder node for.</param>
        /// <returns>An <see langword="ISolutionFolderNode"/> instance if the hierarchy node is supported; <see langword="null"/> otherwise.</returns>
        public virtual ITreeNode CreateNode(Lazy<ITreeNode> parent, IVsSolutionHierarchyNode hierarchy)
		{
			return Supports(hierarchy) ?
				new SolutionNode(hierarchy, childNodeFactory.Value, looseNodeFactory, locator,  adapter, solutionEvents, uiThread) : null;
		}
	}
}