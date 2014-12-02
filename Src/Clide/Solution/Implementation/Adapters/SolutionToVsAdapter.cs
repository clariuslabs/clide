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
	using Clide.Sdk.Solution;
	using Clide.VisualStudio;
	using Microsoft.VisualStudio.Shell.Interop;
	using System;

    [Adapter]
    internal class SolutionToVsAdapter : 
        IAdapter<SolutionTreeNode, VsHierarchyItem>,
        IAdapter<SolutionTreeNode, IVsSolutionHierarchyNode>,
        IAdapter<SolutionTreeNode, IVsHierarchy>,
        IAdapter<SolutionNode, IVsSolution>,
        IAdapter<ProjectNode, IVsProject>
    {
        VsHierarchyItem IAdapter<SolutionTreeNode, VsHierarchyItem>.Adapt(SolutionTreeNode from)
        {
            return new VsHierarchyItem(from.HierarchyNode.VsHierarchy, from.HierarchyNode.ItemId);
        }

        IVsSolutionHierarchyNode IAdapter<SolutionTreeNode, IVsSolutionHierarchyNode>.Adapt(SolutionTreeNode from)
        {
            return from.HierarchyNode;
        }

        IVsHierarchy IAdapter<SolutionTreeNode, IVsHierarchy>.Adapt(SolutionTreeNode from)
        {
            return from.HierarchyNode.VsHierarchy;
        }

        public IVsSolution Adapt(SolutionNode from)
        {
            return from.HierarchyNode.ServiceProvider.GetService<SVsSolution, IVsSolution>();
        }

        public IVsProject Adapt(ProjectNode from)
        {
            return from.HierarchyNode.VsHierarchy as IVsProject;
        }
	}
}
