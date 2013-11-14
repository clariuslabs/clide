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

namespace Clide.Solution
{
    using Clide.Patterns.Adapter;
    using System;
    using System.Dynamic;
    using Clide.VisualStudio;
    using EnvDTE;
    using Microsoft.VisualStudio;

    internal class SolutionItemNode : SolutionTreeNode, ISolutionItemNode
    {
        private ITreeNodeFactory<IVsSolutionHierarchyNode> nodeFactory;
        private Lazy<ISolutionFolderNode> owningFolder;

        public SolutionItemNode(
            IVsSolutionHierarchyNode hierarchyNode,
            Lazy<ITreeNode> parentNode,
            ITreeNodeFactory<IVsSolutionHierarchyNode> nodeFactory,
            IAdapterService adapter)
            : base(SolutionNodeKind.SolutionItem, hierarchyNode, parentNode, nodeFactory, adapter)
        {
            Guard.NotNull(() => parentNode, parentNode);

            this.nodeFactory = nodeFactory;

            this.Item = new Lazy<EnvDTE.ProjectItem>(
                () => (EnvDTE.ProjectItem)hierarchyNode.VsHierarchy.Properties(hierarchyNode.ItemId).ExtenderObject);

            this.owningFolder = new Lazy<ISolutionFolderNode>(() =>
            {
                var owningHierarchy = new VsSolutionHierarchyNode(hierarchyNode.VsHierarchy, VSConstants.VSITEMID_ROOT);
                return this.nodeFactory.CreateNode(GetParent(owningHierarchy), owningHierarchy) as ISolutionFolderNode;
            });
        }

        public Lazy<ProjectItem> Item { get; private set; }

        public string PhysicalPath
        {
            get { return this.Item.Value.get_FileNames(1); }
        }

        public ISolutionFolderNode OwningSolutionFolder
        {
            get { return this.owningFolder.Value; }
        }

        public override bool Accept(ISolutionVisitor visitor)
        {
            return SolutionVisitable.Accept(this, visitor);
        }

        private Lazy<ITreeNode> GetParent(IVsSolutionHierarchyNode hierarchy)
        {
            return hierarchy.Parent == null ? null :
               new Lazy<ITreeNode>(() => this.nodeFactory.CreateNode(GetParent(hierarchy.Parent), hierarchy.Parent));
        }
    }
}