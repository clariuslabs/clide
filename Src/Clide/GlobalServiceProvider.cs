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
    using System.ComponentModel.Composition;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Microsoft.VisualStudio.ComponentModelHost;
    using Microsoft.VisualStudio.Shell;

    /// <summary>
    /// Locates global services inside Visual Studio, in a thread-safe way, unlike 
    /// the VS Shell version.
    /// </summary>
    public static class GlobalServiceProvider
    {
        private static readonly IServiceProvider dteProvider = new DteServiceProvider();
        private static readonly VsServiceProvider vsProvider = new VsServiceProvider();
        private static readonly ComponentModelProvider cmProvider = new ComponentModelProvider(vsProvider);

        private static IServiceProvider globalProvider = new FallbackServiceProvider(
            // DTE, ComponentModel, Global Package
            dteProvider, new FallbackServiceProvider(
                cmProvider, 
                    vsProvider));

        /// <summary>
        /// Gets the global service provider.
        /// </summary>
        public static IServiceProvider Instance
        {
            get { return globalProvider; }
        }

        private class FallbackServiceProvider : IServiceProvider
        {
            private IServiceProvider primary;
            private IServiceProvider fallback;

            public FallbackServiceProvider(IServiceProvider primary, IServiceProvider fallback)
            {
                this.primary = primary;
                this.fallback = fallback;
            }

            public object GetService(Type serviceType)
            {
                return primary.GetService(serviceType) ?? fallback.GetService(serviceType);
            }
        }

        private class DteServiceProvider : IServiceProvider
        {
            private static Lazy<IServiceProvider> globalProvider = new Lazy<IServiceProvider>(GetGlobalProvider);

            public object GetService(Type serviceType)
            {
                return globalProvider.Value.GetService(serviceType);
            }

            private static IServiceProvider GetGlobalProvider()
            {
                var dte = Package.GetGlobalService(typeof(EnvDTE.DTE));
                var ole = dte as Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
                return new Microsoft.VisualStudio.Shell.ServiceProvider(ole);
            }
        }

        private class ComponentModelProvider : IServiceProvider
        {
            private static MethodInfo getService = typeof(IComponentModel).GetMethod("GetService");
            private IComponentModel componentModel;

            public ComponentModelProvider(IServiceProvider parentProvider)
            {
                componentModel = parentProvider.GetService<SComponentModel, IComponentModel>();
            }

            public object GetService(Type serviceType)
            {
                // TODO: cache delegates per service type for performance.
                try
                {
                    return getService.MakeGenericMethod(serviceType).Invoke(componentModel, null);
                }
                catch (Exception)
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