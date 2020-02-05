using System;

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
    /// A service provider that will try a primary service provider and 
    /// if the requested service type is not found, will fall back to 
    /// a secondary one.
    /// </summary>
    internal class FallbackServiceProvider : IServiceProvider
    {
        private IServiceProvider primary;
        private IServiceProvider fallback;

        /// <summary>
        /// Initializes a new instance of the <see cref="FallbackServiceProvider"/> class.
        /// </summary>
        /// <param name="primary">The primary service provider.</param>
        /// <param name="fallback">The fallback service provider.</param>
        public FallbackServiceProvider(IServiceProvider primary, IServiceProvider fallback)
        {
            this.primary = primary;
            this.fallback = fallback;
        }

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <returns>
        /// A service object of type <paramref name="serviceType" />.-or- null if there is no service object of type <paramref name="serviceType" />.
        /// </returns>
        public object GetService(Type serviceType)
        {
            return primary.GetService(serviceType) ?? fallback.GetService(serviceType);
        }
    }
}
