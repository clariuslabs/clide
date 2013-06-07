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
    /// This factory caches the map between hierarchy items and the concrete 
    /// node factories that support them, so that the node creation is 
    /// very fast.
    /// </summary>
	internal class AggregateHierarchyFactory : ITreeNodeFactory<IVsSolutionHierarchyNode>
	{
		private Lazy<ITreeNodeFactory<IVsSolutionHierarchyNode>> factory;

        public AggregateHierarchyFactory(IEnumerable<Lazy<ITreeNodeFactory<IVsSolutionHierarchyNode>, TreeNodeFactoryMetadata>> nodeFactories)
		{
            this.factory = new Lazy<ITreeNodeFactory<IVsSolutionHierarchyNode>>(() => new FallbackNodeFactory<IVsSolutionHierarchyNode>(
                    new CachingNodeFactory(nodeFactories.Where(n => !n.Metadata.IsFallback).Select(f => f.Value)),
                    new CachingNodeFactory(nodeFactories.Where(n => n.Metadata.IsFallback).Select(f => f.Value))));
		}

		public bool Supports(IVsSolutionHierarchyNode hierarchy)
		{
            // TODO: cache by IVsHierarchy->Factory mapping

			return this.factory.Value.Supports(hierarchy);
		}

		public ITreeNode CreateNode(Lazy<ITreeNode> parent, IVsSolutionHierarchyNode hierarchy)
		{
            // TODO: cache by IVsHierarchy->Factory mapping.
            
            return this.factory.Value.CreateNode(parent, hierarchy);
		}

        internal class CachingNodeFactory : ITreeNodeFactory<IVsSolutionHierarchyNode>
        {
            private Dictionary<Tuple<int, uint>, ITreeNodeFactory<IVsSolutionHierarchyNode>> hierarchyToFactoryMap =
                new Dictionary<Tuple<int, uint>, ITreeNodeFactory<IVsSolutionHierarchyNode>>();

            private List<ITreeNodeFactory<IVsSolutionHierarchyNode>> factories;

            public CachingNodeFactory(IEnumerable<ITreeNodeFactory<IVsSolutionHierarchyNode>> nodeFactories)
            {
                this.factories = nodeFactories.ToList();
            }

            public bool Supports(IVsSolutionHierarchyNode model)
            {
                return this.FindFactory(model) != null;
            }

            public ITreeNode CreateNode(Lazy<ITreeNode> parent, IVsSolutionHierarchyNode model)
            {
                var factory = this.FindFactory(model);

                return factory == null ? null : factory.CreateNode(parent, model);
            }

            private ITreeNodeFactory<IVsSolutionHierarchyNode> FindFactory(IVsSolutionHierarchyNode model)
            {
                ITreeNodeFactory<IVsSolutionHierarchyNode> mapped;
                var key = Tuple.Create(model.VsHierarchy.GetHashCode(), model.ItemId);
                if (this.hierarchyToFactoryMap.TryGetValue(key, out mapped))
                    // We will add a null entry the first time we see the hierarchy item 
                    // so that we avoid probing for a factory again if it's not supported.
                    return mapped;

                var factory = this.factories.FirstOrDefault(x => x.Supports(model));
                // We always add the entry, even if it's null.
                this.hierarchyToFactoryMap[key] = factory;

                return factory;
            }
        }
	}
}
