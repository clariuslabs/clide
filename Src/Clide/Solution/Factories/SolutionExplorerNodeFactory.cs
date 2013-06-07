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

namespace Clide.Solution.Factories
{
    using System;
    using System.ComponentModel.Composition;
    using Clide.Composition;

    [Component(typeof(ISolutionExplorerNodeFactory))]
    internal class SolutionExplorerNodeFactory : ISolutionExplorerNodeFactory
    {
        private Lazy<ITreeNodeFactory<IVsSolutionHierarchyNode>> nodeFactory;

        public SolutionExplorerNodeFactory([WithKey(DefaultHierarchyFactory.RegisterKey)] Lazy<ITreeNodeFactory<IVsSolutionHierarchyNode>> nodeFactory)
        {
            this.nodeFactory = nodeFactory;
        }

        public ISolutionExplorerNode Create(IVsSolutionHierarchyNode hierarchyNode)
        {
            Func<IVsSolutionHierarchyNode, Lazy<ITreeNode>> getParent = null;
            Func<IVsSolutionHierarchyNode, ITreeNode> getNode = null;

            getNode = hierarchy => hierarchy == null ? null :
                this.nodeFactory.Value.CreateNode(getParent(hierarchy), hierarchy);

            getParent = hierarchy => hierarchy.Parent == null ? null :
                new Lazy<ITreeNode>(() => this.nodeFactory.Value.CreateNode(getParent(hierarchy.Parent), hierarchy.Parent));

            return getNode(hierarchyNode) as ISolutionExplorerNode;        
        }
    }
}
