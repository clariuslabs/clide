using System;
using Clide.Interop;
using Microsoft.VisualStudio.Shell;

namespace Clide
{
    /// <summary>
    /// Locates global services inside Visual Studio, in a thread-agnostic way, 
	/// unlike the VS Shell version. To also retrieve components exposed via MEF,
    /// use the <see cref="ServiceLocator"/> instead.
    /// </summary>
    public static class ServiceProvider
    {
        static readonly IServiceProvider dteProvider = new DteServiceProvider();
        static readonly VsServiceProvider vsProvider = new VsServiceProvider();

        static IServiceProvider globalProvider = new FallbackServiceProvider(dteProvider, vsProvider);

        /// <summary>
        /// Gets the global service provider.
        /// </summary>
        public static IServiceProvider Global
        {
            get { return globalProvider; }
        }

        private class DteServiceProvider : IServiceProvider
        {
			private static Lazy<IServiceProvider> globalProvider = new Lazy<IServiceProvider>(() => GetGlobalProvider());

            public object GetService(Type serviceType)
            {
                return globalProvider.Value.GetService(serviceType);
            }

            private static IServiceProvider GetGlobalProvider()
            {
                var dte = Package.GetGlobalService(typeof(EnvDTE.DTE));
				if (dte == null) {
					dte = ThreadHelper.Generic.Invoke (() => Package.GetGlobalService (typeof (EnvDTE.DTE)));
					if (dte == null)
						dte = RunningObjects.GetDTE (TimeSpan.FromMilliseconds (500));
				}

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