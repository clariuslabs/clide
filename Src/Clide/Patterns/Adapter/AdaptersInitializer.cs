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

    /// <summary>
    /// Provides the entry point for setting the implementation of the 
    /// <see cref="IAdapterService"/>, typically used only by bootstrapping 
    /// code.
    /// </summary>
    /// <remarks>
    /// Startup code in the application should invoke the <see cref="SetService"/> method
    /// before any adaptations are performed. Alternatively, a transient service API 
    /// is provided that allows specific contexts to override the global implementation, 
    /// typically in multi-threaded test runs, which invoke the <see cref="SetTransientService"/> 
    /// method instead. 
    /// <para>
    /// This transient instance remains the current adapter service for the 
    /// duration of the call, including spanned threads or tasks.
    /// </para>
    /// </remarks>
    public static class AdaptersInitializer
    {
        private static readonly AmbientSingleton<IAdapterService> transientService = new AmbientSingleton<IAdapterService>(new Guid(Constants.TransientStateIdenfier));

        /// <summary>
        /// Sets the singleton adapter service instance to use to implement the 
        /// <see cref="As"/> extension method for the entire lifetime of the 
        /// current application domain.
        /// </summary>
        public static void SetService(IAdapterService service)
        {
            if (AppDomain.CurrentDomain.GetData(Constants.GlobalStateIdentifier) != null)
                throw new NotSupportedException("Global adapter service can only be set once per application domain.");

            AppDomain.CurrentDomain.SetData(Constants.GlobalStateIdentifier, service);
        }

        /// <summary>
        /// Sets up a transient adapter service that remains active during 
        /// an entire call chain, even across code that spawns new threads 
        /// or tasks, but does not overwrite the global singleton service 
        /// specified via <see cref="SetService"/>.
        /// </summary>
        /// <returns>A disposable object that removes the transient service when disposed.</returns>
        /// <remarks>
        /// Typical usage includes placing the call in a using statement:
        /// <code>
        /// using (AdaptersInitializer.SetTransientService(serviceMock))
        /// {
        ///   // Invoke code that uses the adapter service.
        /// }
        /// </code>
        /// </remarks>
        public static IDisposable SetTransientService(IAdapterService service)
        {
            transientService.Value = service;
            return new Disposable(() => transientService.Value = null);
        }

        private class Disposable : IDisposable
        {
            private Action dispose;

            public Disposable(Action dispose)
            {
                this.dispose = dispose;
            }

            public void Dispose()
            {
                this.dispose.Invoke();    
            }
        }
    }
}
