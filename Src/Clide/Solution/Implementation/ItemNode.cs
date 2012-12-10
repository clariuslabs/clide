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
    using Clide.Patterns.Adapter;
    using System;
    using System.Dynamic;
    using Clide.VisualStudio;
    using EnvDTE;

    internal class ItemNode : SolutionTreeNode, IItemNode
    {
        public ItemNode(
            IVsSolutionHierarchyNode hierarchyNode,
            Lazy<ITreeNode> parentNode,
            ITreeNodeFactory<IVsSolutionHierarchyNode> nodeFactory,
            IAdapterService adapter)
            : base(SolutionNodeKind.Item, hierarchyNode, parentNode, nodeFactory, adapter)
        {
            this.Item = new Lazy<EnvDTE.ProjectItem>(
                () => (EnvDTE.ProjectItem)hierarchyNode.VsHierarchy.Properties(hierarchyNode.ItemId).ExtenderObject);
        }

        public Lazy<ProjectItem> Item { get; private set; }

        public string PhysicalPath
        {
            get { return this.Item.Value.get_FileNames(1); }
        }

        public dynamic Data
        {
            // TODO: implement
            get { return new ExpandoObject(); }
        }
    }
}