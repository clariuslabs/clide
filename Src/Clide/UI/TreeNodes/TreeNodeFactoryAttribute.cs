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
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Composition;
    using Clide.Composition;

    /// <summary>
    /// Attribute that designates a given class as a 
    /// tree node factory for a specific owner and model type.
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class TreeNodeFactoryAttribute : ComponentAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TreeNodeFactoryAttribute"/> class.
        /// </summary>
        /// <param name="treeOwner">The owner of the tree, used to filter factories for this particular tree.</param>
        /// <param name="modelType">Type of the model backing the tree.</param>
        /// <param name="isFallback">Whether the factory is a fallback one, meaning a non-fallback one should be tried first..</param>
        public TreeNodeFactoryAttribute(string treeOwner, Type modelType, bool isFallback)
            : base(treeOwner, BuildFactoryType(modelType))
        {
            this.IsFallback = isFallback;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeNodeFactoryAttribute"/> class.
        /// </summary>
        /// <param name="treeOwner">The owner of the tree, used to filter factories for this particular tree.</param>
        /// <param name="modelType">Type of the model backing the tree.</param>
        /// <param name="isFallback">Whether the factory is a fallback one, meaning a non-fallback one should be tried first..</param>
        public TreeNodeFactoryAttribute(Type treeOwner, Type modelType, bool isFallback)
            : base(ContractFor(treeOwner), BuildFactoryType(modelType))
        {
            this.IsFallback = isFallback;
        }

        /// <summary>
        /// For internal use by the composition container.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public TreeNodeFactoryAttribute(IDictionary<string, object> metadata)
            : base(metadata)
        {
            object value;
            if (metadata.TryGetValue("IsFallback", out value))
                this.IsFallback = (bool)value;
        }

        /// <summary>
        /// Gets a value indicating whether the factory is meant
        /// to provide fallback behavior.
        /// </summary>
        public bool IsFallback { get; private set; }

        private static string ContractFor(Type treeOwner)
        {
            return treeOwner.FullName;
        }

        private static Type BuildFactoryType(Type modelType)
        {
            return typeof(ITreeNodeFactory<>).MakeGenericType(modelType);
        }
    }
}
