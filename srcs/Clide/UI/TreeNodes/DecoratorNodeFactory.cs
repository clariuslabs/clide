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

namespace Clide
{
    using System.Collections.Generic;
    using System.Linq;
    using System;

    internal class DecoratorNodeFactory<TModel> : ITreeNodeFactory<TModel>
	{
		public DecoratorNodeFactory(
			ITreeNodeFactory<TModel> factory,
			IEnumerable<ITreeNodeDecorator> decorators)
		{
			this.NodeFactory = factory;
			this.NodeDecorators = decorators.ToList();
		}

		public ITreeNodeFactory<TModel> NodeFactory { get; private set; }
		public IEnumerable<ITreeNodeDecorator> NodeDecorators { get; private set; }

		public bool Supports(TModel hierarchy)
		{
			return this.NodeFactory.Supports(hierarchy);
		}

		public ITreeNode CreateNode(Lazy<ITreeNode> parent, TModel hierarchy)
		{
			// Note: rather than querying for Supports(node) and later calling Create(node), we 
			// rely on a behavior such that if the factory cannot create a node, it will return null. 
			// This is faster. We implement that behavior on both the fallback factory as well as 
			// the aggregate factory.

			var node = this.NodeFactory.CreateNode(parent, hierarchy);

			if (node != null)
			{
				this.NodeDecorators
					.Where(decorator => decorator.Supports(node))
					// Aggregate essentially replaces the current node returned by 
					// each decorator until we're done.
					.Aggregate(node, (current, decorator) => decorator.Decorate(current));
			}

			return node;
		}
	}
}
