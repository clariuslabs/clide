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
    /// <summary>
    /// Provides the basic non-generic protocol for the reference provider.
    /// Implementations should inherit from <see cref="IReferenceProvider{T}"/>.
    /// </summary>
    /// <remarks>
    /// A reference provider is responsible for converting from object instances 
    /// to references like file://foo.txt. The <see cref="Scheme"/> determines 
    /// which provider will be invoked to resolve which references, as well 
    /// as the type to be resolved. 
    /// </remarks>
    public interface IReferenceProvider
    {
        /// <summary>
        /// Gets the scheme of the provider. All created references 
        /// from this provider must use this scheme followed by a colon.
        /// </summary>
        /// <remarks>
        /// The scheme is case-sensitive when locating the provider for 
        /// a given reference.
        /// </remarks>
        string Scheme { get; }
    }
}