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
    /// <summary>
    /// Shared constants across implementation and interface.
    /// </summary>
    internal static partial class Constants
    {
        private static string globalIdentifier;
        private static string transientIdentifier;

        static Constants()
        {
            globalIdentifier = typeof(IAdapterService).Assembly.GetName().Version + "{29938042-C16D-46BA-93D3-F513E88EC345}";
            transientIdentifier = typeof(IAdapterService).Assembly.GetName().Version + "{E0A43955-6A73-4FD7-B30E-3FB699A804EC}";
        }

        /// <summary>
        /// The identifier for the global state that lives in the AppDomain and provides 
        /// the implementation for the extension method at runtime.
        /// </summary>
        /// <remarks>
        /// The current assembly version is always prepended to this identifier to 
        /// avoid collisions in the AppDomain if multiple versions of the adapter 
        /// are running side-by-side.
        /// </remarks>
        public static string GlobalStateIdentifier
        {
            get { return globalIdentifier; }
        }

        /// <summary>
        /// The identifier for the transient state that lives in an AmbientSingleton that 
        /// can be used in tests to replace the adapter service.
        /// </summary>
        /// <remarks>
        /// The current assembly version is always prepended to this identifier to 
        /// avoid collisions in the AppDomain if multiple versions of the adapter 
        /// are running side-by-side.
        /// </remarks>
        public static string TransientStateIdenfier
        {
            get { return transientIdentifier; }
        }
    }
}