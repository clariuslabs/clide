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
    /// Provides a general mechanism to create reference handles to objects 
    /// and to resolve them to their referenced instance later.
    /// </summary>
    /// <remarks>
    /// This service manages references by allowing creation, resolution and 
    /// opening of those references. Reference providers implementing 
    /// <see cref="IReferenceProvider{T}"/> are registered with the 
    /// <see cref="IReferenceProviderRegistry"/>, which is used by this service 
    /// to determine which providers to invoke according to the received instance 
    /// types or the optional scheme passed to the relevant methods.
    /// <para>
    /// The references are intentionally strings and not <see cref="Uri"/> because the 
    /// <see cref="Uri"/> type imposes more strict rules than are required for 
    /// a general-purpose private referencing service, since it adheres to the 
    /// internet standard, which is beyond the requirements for this general-purpose 
    /// private referencing system.
    /// </para>
    /// <para>
    /// This service will attempt to find the most appropriate provider according 
    /// to the instance types.
    /// </para>
    /// <para>
    /// The service also ensures that providers return valid references according 
    /// to their specified <see cref="IReferenceProvider.Scheme"/>.
    /// </para>
    /// </remarks>
    public interface IReferenceService
    {
        /// <summary>
        /// Tries to create a reference for the instance of T.
        /// </summary>
        /// <typeparam name="T">The type of the instance to create a reference for.</typeparam>
        /// <param name="instance">The instance to create a reference for.</param>
        /// <param name="scheme">The optional scheme to use to determine how the reference should be created. If 
        /// none is specified, a registered <see cref="IReferenceProvider{T}"/> of the given
        /// <typeparamref name="T"/> would create the reference.</param>
        /// <returns>The reference to the instance or <see langword="null"/> if no registered 
        /// provider could create the reference.</returns>
        string TryCreateReference<T>(T instance, string scheme = null) where T : class;

        /// <summary>
        /// Tries to open the instance in the appropiate view.
        /// </summary>
        /// <typeparam name="T">The type of the instance to open.</typeparam>
        /// <param name="instance">The instance to open in its default view.</param>
        /// <param name="scheme">The optional scheme to use to determine how the reference should be opened. 
        /// If none is specified, a registered <see cref="IReferenceProvider{T}"/> of the given
        /// <typeparamref name="T"/> would open the default view.</param>
        /// <returns><see langword="true"/> if the instance could be opened; <see langword="false"/> otherwise.</returns>
        bool TryOpen<T>(T instance, string scheme = null) where T : class;

        /// <summary>
        /// Tries to resolve the given reference to an instance of T based on the 
        /// reference scheme and the registered reference providers.
        /// </summary>
        /// <typeparam name="T">The type of the instance to be resolved</typeparam>
        /// <param name="reference">The reference to try to resolve.</param>
        /// <returns>The resolved reference or <see langword="null"/> if it could 
        /// not be resolved to a valid instance of <typeparamref name="T"/>.</returns>
        /// <exception cref="ArgumentException">The <paramref name="reference"/> does not start with a scheme followed by a colon.</exception>
        T TryResolveReference<T>(string reference) where T : class;
    }
}