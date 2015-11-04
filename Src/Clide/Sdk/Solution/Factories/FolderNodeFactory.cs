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
    using Clide.Patterns.Adapter;
    using Clide.Solution.Implementation;
    using Clide.VisualStudio;
    using System;

    /// <summary>
    /// Factory for <see cref="Clide.Solution.IFolderNode"/> in managed projects.
    /// </summary>
    [Named("SolutionExplorer")]
    [FallbackFactory]
    public class FolderNodeFactory : ITreeNodeFactory<IVsSolutionHierarchyNode>
    {
        private Lazy<ITreeNodeFactory<IVsSolutionHierarchyNode>> childNodeFactory;
        private IAdapterService adapter;

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderNodeFactory"/> class.
        /// </summary>
        /// <param name="childNodeFactory">The factory for nodes, used to construct child nodes automatically.</param>
        /// <param name="adapter">The adapter service that implements the smart cast <see cref="ITreeNode.As{T}"/>.</param>
        public FolderNodeFactory(
            [Named(DefaultHierarchyFactory.RegisterKey)] Lazy<ITreeNodeFactory<IVsSolutionHierarchyNode>> childNodeFactory,
            IAdapterService adapter)
        {
            this.childNodeFactory = childNodeFactory;
            this.adapter = adapter;
        }

        /// <summary>
        /// Determines whether this factory supports the given hierarchy node.
        /// </summary>
        /// <param name="hierarchy">The hierarchy node to check.</param>
        /// <returns><see langword="true"/> if the given node is a folder supported by this factory; <see langword="false"/> otherwise.</returns>
        public virtual bool Supports(IVsSolutionHierarchyNode hierarchy)
        {
            var extenderObject = hierarchy.VsHierarchy.Properties(hierarchy.ItemId).ExtenderObject;
            var projectItem = extenderObject as EnvDTE.ProjectItem;

			if (extenderObject == null || projectItem == null)
				return false;

			if (extenderObject.GetType ().FullName == "Microsoft.VisualStudio.Project.Automation.OAFolderItem")
				return true;

			try {
				// Fails in F# projects.
				return projectItem.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFolder;
            } catch (Exception) {
				return false;
			}
        }

        /// <summary>
        /// Creates the folder node if supported by this factory.
        /// </summary>
        /// <param name="parent">The accessor for the parent node of this folder.</param>
        /// <param name="hierarchy">The hierarchy node to construct the folder node for.</param>
        /// <returns>An <see langword="IFolderNode"/> instance if the hierarchy node is supported; <see langword="null"/> otherwise.</returns>
        public virtual ITreeNode CreateNode(Lazy<ITreeNode> parent, IVsSolutionHierarchyNode hierarchy)
        {
            return Supports(hierarchy) ?
                 new FolderNode(hierarchy, parent, this.childNodeFactory.Value, this.adapter) : null;
        }
    }
}
