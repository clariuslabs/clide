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

    /// <summary>
    /// An aggregate factory that delegates the <see cref="Supports"/> and 
    /// <see cref="CreateNode"/> implementations to the first factory 
    /// received in the constructor that supports the given model node.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    internal class AggregateNodeFactory<TModel> : ITreeNodeFactory<TModel>
	{
		public AggregateNodeFactory(IEnumerable<ITreeNodeFactory<TModel>> nodeFactories)
		{
			this.NodeFactories = nodeFactories.ToList();
		}

		public IEnumerable<ITreeNodeFactory<TModel>> NodeFactories { get; private set; }

		public bool Supports(TModel model)
		{
			return this.NodeFactories.Any(factory => factory.Supports(model));
		}

		public ITreeNode CreateNode(Lazy<ITreeNode> parent, TModel model)
		{
			var factory = this.NodeFactories.FirstOrDefault(f => f.Supports(model));

			return factory == null ? null : factory.CreateNode(parent, model);
		}
	}
}
