using System;
using Merq;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide
{
    /// <summary>
    /// Provides easy access to the <see cref="Global"/> service 
    /// locator, as well as package-specific service locator. 
    /// Complements the <c>GetServiceLocator</c> extension methods 
    /// provided for most common Visual Studio entry points such 
    /// as <see cref="EnvDTE.DTE"/>, <see cref="EnvDTE.Solution"/>, <see cref="EnvDTE.Project"/>, 
    /// <see cref="IVsHierarchy"/> and <see cref="IVsProject"/>.
    /// </summary>
    public static class ServiceLocator
    {
        static Lazy<IServiceLocator> globalLocator = new Lazy<IServiceLocator>(() => ServiceProvider.Global.GetServiceLocator());

        /// <summary>
        /// Accesses the global service locator for the global service 
        /// </summary>
        public static IServiceLocator Global { get { return globalLocator.Value; } }

        /// <summary>
        /// Loads the given package and retrieves the <see cref="IServiceLocator"/> for it.
        /// </summary>
        public static IServiceLocator Get(string packageGuid)
        {
            return Get(new Guid(packageGuid));
        }

        /// <summary>
        /// Loads the given package and retrieves the <see cref="IServiceLocator"/> for it.
        /// </summary>
        public static IServiceLocator Get(Guid packageGuid)
        {
            var async = Global.GetExport<IAsyncManager>();
            return async.Run(async () =>
            {
                var vsPackage = default(IVsPackage);
                await async.SwitchToMainThread();

                var vsShell = ServiceProvider.Global.GetService<SVsShell, IVsShell>();
                vsShell.IsPackageLoaded(ref packageGuid, out vsPackage);

                if (vsPackage == null)
                    ErrorHandler.ThrowOnFailure(vsShell.LoadPackage(ref packageGuid, out vsPackage));

                return ((IServiceProvider)vsPackage).GetServiceLocator();
            });
        }
    }
}
