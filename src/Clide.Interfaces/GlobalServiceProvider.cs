namespace Clide
{
    using Microsoft.VisualStudio.Shell;
    using System;

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