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
    /// Allows to create and resolve references for the target type T.
    /// </summary>
    /// <typeparam name="T">The type of instance to be resolved or referenced</typeparam>
    /// <remarks>
    /// The <see cref="TryCreateReference"/>, if it succeeds, must always 
    /// return references that start with the <see cref="IReferenceProvider.Scheme"/> 
    /// followed by a colon, like vsix://my-vsix-identifier or project:my-project-guid
    /// <para>
    /// The references are intentionally strings and not <see cref="Uri"/> because the 
    /// <see cref="Uri"/> type imposes more strict rules than are required for 
    /// a general-purpose private referencing service.
    /// </para>
    /// </remarks>
    public interface IReferenceProvider<T> : IReferenceProvider
        where T : class
    {
        /// <summary>
        /// Tries to create a reference for the instance of T.
        /// </summary>
        /// <param name="instance">The instance to create a reference for.</param>
        /// <returns>The reference to the instance or <see langword="null"/> if a reference 
        /// cannot be created for the given instance.</returns>
        string TryCreateReference(T instance);

        /// <summary>
        /// Tries to open the instance in the appropiate view.
        /// </summary>
        /// <param name="instance">The instance to open in its default view.</param>
        /// <returns><see langword="true"/> if the instance could be opened; <see langword="false"/> otherwise.</returns>
        bool TryOpen(T instance);

        /// <summary>
        /// Tries to resolve the given reference to an instance of T.
        /// </summary>
        /// <param name="reference">The reference to try to resolve.</param>
        /// <returns>The resolved reference or <see langword="null"/> if it could 
        /// not be resolved to a valid instance of <typeparamref name="T"/>.</returns>
        T TryResolveReference(string reference);
    }
}