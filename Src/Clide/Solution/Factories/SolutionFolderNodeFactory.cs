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
    using System;
    using Clide.Patterns.Adapter;
    using Clide.VisualStudio;
    using Clide.Composition;

    [FallbackFactory]
    internal class SolutionFolderNodeFactory : ITreeNodeFactory<IVsSolutionHierarchyNode>
	{
		private Lazy<ITreeNodeFactory<IVsSolutionHierarchyNode>> nodeFactory;
		private IAdapterService adapter;

		public SolutionFolderNodeFactory(
			[WithKey(DefaultHierarchyFactory.RegisterKey)] Lazy<ITreeNodeFactory<IVsSolutionHierarchyNode>> nodeFactory,
			IAdapterService adapter)
		{
			this.nodeFactory = nodeFactory;
			this.adapter = adapter;
		}

		public bool Supports(IVsSolutionHierarchyNode hierarchy)
		{
			var project = hierarchy.VsHierarchy.Properties(hierarchy.ItemId).ExtenderObject as EnvDTE.Project;

			return project != null &&
				project.Object is EnvDTE80.SolutionFolder;
		}

		public ITreeNode CreateNode(Lazy<ITreeNode> parent, IVsSolutionHierarchyNode hierarchy)
		{
			return Supports(hierarchy) ?
				new SolutionFolderNode(hierarchy, parent, this.nodeFactory.Value, this.adapter) : null;
		}
	}
}