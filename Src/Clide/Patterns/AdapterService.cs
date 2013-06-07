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

namespace Clide.Patterns.Adapter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Clide.Composition;

    [Component(typeof(IAdapterService))]
    partial class AdapterService
    {
        /// <summary>
        /// This method is used only for diagnostics purposes.
        /// </summary>
        internal IEnumerable<Type> GetSupportedConversions(Type fromType)
        {
            var fromInheritance = GetInheritance(fromType);

            return this.allAdapters
                // Filter out those that are compatible both for the source and the target.
                .Where(info => info.From.IsAssignableFrom(fromType))
                .Select(info => new
                {
                    // Gets the distance between the requested From type to the adapter From type.
                    FromInheritance = fromInheritance.FirstOrDefault(x => x.Type == info.From),
                    // Gets the distance between the requested To type to the adapter To type.
                    ToInheritance = GetInheritance(info.To)
                })
                .SelectMany(info => info.ToInheritance.Select(h => h.Type));
        }
    }
}
