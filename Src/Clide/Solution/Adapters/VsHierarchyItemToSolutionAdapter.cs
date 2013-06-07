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

namespace Clide.Solution.Adapters
{
    using Clide.Patterns.Adapter;
    using System.ComponentModel.Composition;

    [Adapter]
    internal class VsHierarchyItemToSolutionAdapter :
        IAdapter<VsHierarchyItem, ISolutionNode>,
        IAdapter<VsHierarchyItem, IProjectNode>, 
        IAdapter<VsHierarchyItem, IItemNode>
    {
        private ISolutionExplorerNodeFactory nodeFactory;

        public VsHierarchyItemToSolutionAdapter(ISolutionExplorerNodeFactory nodeFactory)
        {
            this.nodeFactory = nodeFactory;
        }

        ISolutionNode IAdapter<VsHierarchyItem, ISolutionNode>.Adapt(VsHierarchyItem from)
        {
            return CreateNode<ISolutionNode>(from);
        }

        IProjectNode IAdapter<VsHierarchyItem, IProjectNode>.Adapt(VsHierarchyItem from)
        {
            return CreateNode<IProjectNode>(from);
        }

        IItemNode IAdapter<VsHierarchyItem, IItemNode>.Adapt(VsHierarchyItem from)
        {
            return CreateNode<IItemNode>(from);
        }

        private TNode CreateNode<TNode>(VsHierarchyItem item)
            where TNode : class
        {
            return this.nodeFactory.Create(new VsSolutionHierarchyNode(
                item.VsHierarchy, item.ItemId))
                as TNode;
        }
    }
}
