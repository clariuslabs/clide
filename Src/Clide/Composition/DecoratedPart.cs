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

namespace Clide.Composition
{
    using System.ComponentModel.Composition.Primitives;
    using System.ComponentModel.Composition.ReflectionModel;
    using System.Collections.Generic;
    using System;

    /// <summary>
    /// The context where a reflection-based part was found in the decorated catalog.
    /// </summary>
    public class DecoratedPart
    {
        /// <summary>
        /// Initializes the context from a part definition.
        /// </summary>
        internal DecoratedPart(ComposablePartDefinition definition)
        {
            this.PartDefinition = definition;
            this.PartType = ReflectionModelServices.GetPartType(definition);
            this.NewMetadata = new Dictionary<string, object>(definition.Metadata);
        }

        /// <summary>
        /// Gets a read/write bag of metadata containing the 
        /// original part metadata.
        /// </summary>
        public IDictionary<string, object> NewMetadata { get; private set; }

        /// <summary>
        /// Gets the original part definition.
        /// </summary>
        public ComposablePartDefinition PartDefinition { get; private set; }

        /// <summary>
        /// Gets the part type.
        /// </summary>
        public Lazy<Type> PartType { get; private set; }
    }
}
