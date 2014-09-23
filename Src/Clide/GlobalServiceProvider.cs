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
    using Microsoft.VisualStudio.ComponentModelHost;
    using Microsoft.VisualStudio.Shell;
    using System;
    using System.Reflection;

    /// <summary>
    /// Locates global services inside Visual Studio, in a thread-safe way, unlike 
    /// the VS Shell version. To also retrieve components exposed via MEF, 
    /// use the <see cref="ServiceLocator"/> instead.
    /// </summary>
    public static class GlobalServiceProvider
    {
        private static readonly IServiceProvider dteProvider = new DteServiceProvider();
        private static readonly VsServiceProvider vsProvider = new VsServiceProvider();

        private static IServiceProvider globalProvider = new FallbackServiceProvider(dteProvider, vsProvider);

        /// <summary>
        /// Gets the global service provider.
        /// </summary>
        public static IServiceProvider Instance
        {
            get { return globalProvider; }
        }

        private class DteServiceProvider : IServiceProvider
        {
			private static IServiceProvider globalProvider = GetGlobalProvider();

            public object GetService(Type serviceType)
            {
                return globalProvider.GetService(serviceType);
            }

            private static IServiceProvider GetGlobalProvider()
            {
                var dte = Package.GetGlobalService(typeof(EnvDTE.DTE));
                var ole = dte as Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
				if (ole == null)
					return new NullServiceProvider();

                return new Microsoft.VisualStudio.Shell.ServiceProvider(ole);
            }

			class NullServiceProvider : IServiceProvider
			{
				public object GetService (Type serviceType)
				{
					return null;
				}
			}
        }

        private class VsServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType)
            {
                return Package.GetGlobalService(serviceType);
            }
        }
    }
}