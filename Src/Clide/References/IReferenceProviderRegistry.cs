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

namespace Clide
{
    using System;

    /// <summary>
    /// Provides a registration mechanism for reference providers 
    /// that are used by the <see cref="IReferenceService"/>.
    /// </summary>
    public interface IReferenceProviderRegistry
    {
        /// <summary>
        /// Registers the specified provider with the service.
        /// </summary>
        /// <param name="provider">The reference provider to register.</param>
        /// <exception cref="ArgumentException">A provider has already been registered 
        /// for the same <see cref="IReferenceProvider.Scheme"/> scheme.</exception>
        void Register(IReferenceProvider provider);

        /// <summary>
        /// Checks if the reference scheme is registered as a valid provider.
        /// </summary>
        /// <param name="scheme">The reference scheme to be checked, such as "vsix" or "project", with or without the trailing ":" or "://".</param>
        /// <returns><see langword="true"/>, if a reference provider exists for the scheme; otherwise <see langword="false"/></returns>
        bool IsRegistered(string scheme);
   }
}