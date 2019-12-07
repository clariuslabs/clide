using Microsoft.VisualStudio.ComponentModelHost;
using System;
namespace Clide
{

    /// <summary>
    /// Entry point to the Clide developer environment APIs.
    /// </summary>
    public static class DevEnv
    {
        public static IDevEnv Get(IServiceProvider serviceProvider) =>
            serviceProvider
                .GetService<SComponentModel, IComponentModel>()
                .GetService<IDevEnv>();

        /// package Clide components.</param>
        public static IDevEnv Get(Guid packageId) =>
            Get(ServiceProvider.Global.GetLoadedPackage(packageId));
    }
}