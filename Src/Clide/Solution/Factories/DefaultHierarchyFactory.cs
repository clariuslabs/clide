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
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using Clide.Composition;

    /// <summary>
    /// This factory is the one that serves as the single dependency 
    /// that all individual node factories reuse for their own 
    /// parent/children construction. 
    /// </summary>
    [Component(RegisterKey, typeof(ITreeNodeFactory<IVsSolutionHierarchyNode>))]
	internal class DefaultHierarchyFactory : ITreeNodeFactory<IVsSolutionHierarchyNode>
	{
        public const string RegisterKey = "Clide.Solution.DefaultHierarchyFactory";

		private Lazy<ITreeNodeFactory<IVsSolutionHierarchyNode>> factory;

		public DefaultHierarchyFactory([WithKey("SolutionExplorer")] 
            IEnumerable<Lazy<ITreeNodeFactory<IVsSolutionHierarchyNode>, TreeNodeFactoryMetadata>> nodeFactories)
		{
            this.factory = new Lazy<ITreeNodeFactory<IVsSolutionHierarchyNode>>(() =>
                new FallbackNodeFactory<IVsSolutionHierarchyNode>(
                    new AggregateNodeFactory<IVsSolutionHierarchyNode>(nodeFactories.Where(n => !n.Metadata.IsFallback).Select(f => f.Value)),
                    new AggregateNodeFactory<IVsSolutionHierarchyNode>(nodeFactories.Where(n => n.Metadata.IsFallback).Select(f => f.Value))));
		}

		public bool Supports(IVsSolutionHierarchyNode hierarchy)
		{
			return this.factory.Value.Supports(hierarchy);
		}

		public ITreeNode CreateNode(Lazy<ITreeNode> parent, IVsSolutionHierarchyNode hierarchy)
		{
            return this.factory.Value.CreateNode(parent, hierarchy);
		}
	}
}
