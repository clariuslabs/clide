using System;
using System.ComponentModel;
using Clide;
using Clide.Properties;
using System.Linq;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;

/// <summary>
/// Defines extension methods related to <see cref="IServiceProvider"/>.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static partial class ServiceProviderExtensions
{
    /// <summary>
    /// Gets type-based services from the  service provider.
    /// </summary>
    /// <nuget id="netfx-System.ServiceProvider" />
    /// <typeparam name="T">The type of the service to get.</typeparam>
    /// <param name="provider" this="true">The service provider.</param>
    /// <returns>The requested service, or a <see langword="null"/> reference if the service could not be located.</returns>
    public static T TryGetService<T>(this IServiceProvider provider)
    {
        Guard.NotNull(nameof(provider), provider);

        return (T)provider.GetService(typeof(T));
    }

    /// <summary>
    /// Gets type-based services from the  service provider.
    /// </summary>
    /// <nuget id="netfx-System.ServiceProvider" />
    /// <typeparam name="T">The type of the service to get.</typeparam>
    /// <param name="provider" this="true">The service provider.</param>
    /// <returns>The requested service, or throws an <see cref="InvalidOperationException"/> 
    /// if the service was not found.</returns>
    public static T GetService<T>(this IServiceProvider provider)
    {
        Guard.NotNull(nameof(provider), provider);

        var service = (T)provider.GetService(typeof(T));
        if (service == null)
            throw new MissingDependencyException(Strings.ServiceLocator.MissingDependency(typeof(T)));

        return service;
    }

    /// <summary>
    /// Gets type-based services from the service provider.
    /// </summary>
    /// <nuget id="netfx-System.ServiceProvider" />
    /// <typeparam name="TRegistration">The type of the registration of the service.</typeparam>
    /// <typeparam name="TService">The type of the service to get.</typeparam>
    /// <param name="provider" this="true">The service provider.</param>
    /// <returns>The requested service, or a <see langword="null"/> reference if the service could not be located.</returns>
    public static TService TryGetService<TRegistration, TService>(this IServiceProvider provider)
    {
        Guard.NotNull(nameof(provider), provider);

        return (TService)provider.GetService(typeof(TRegistration));
    }

    /// <summary>
    /// Gets type-based services from the service provider.
    /// </summary>
    /// <nuget id="netfx-System.ServiceProvider" />
    /// <typeparam name="TRegistration">The type of the registration of the service.</typeparam>
    /// <typeparam name="TService">The type of the service to get.</typeparam>
    /// <param name="provider" this="true">The service provider.</param>
    /// <returns>The requested service, or throws an <see cref="InvalidOperationException"/> 
    /// if the service was not found.</returns>
    public static TService GetService<TRegistration, TService>(this IServiceProvider provider)
    {
        Guard.NotNull(nameof(provider), provider);

        var service = (TService)provider.GetService(typeof(TRegistration));
        if (service == null)
            throw new MissingDependencyException(Strings.ServiceLocator.MissingDependency(typeof(TRegistration)));

        return service;
    }

    static readonly string PackageFullName = typeof(Package).FullName;

    /// <summary>
    /// Determines whether the given service provider is a package.
    /// </summary>
    internal static bool IsPackage(this IServiceProvider serviceProvider)
    {
        var type = serviceProvider.GetType();
        while (type != typeof(object))
        {
            if (type.FullName == PackageFullName)
                return true;

            type = type.BaseType;
        }

        return false;
    }

    /// <summary>
    /// Gets the package GUID or throws an <see cref="ArgumentException"/> if the 
    /// <see cref="GuidAttribute"/> is not found on the given instance type.
    /// </summary>
    internal static Guid GetPackageGuidOrThrow(this IServiceProvider owningPackage)
    {
        var guid = owningPackage.GetType().GetCustomAttributes(typeof(GuidAttribute), true)
            .OfType<GuidAttribute>()
            .FirstOrDefault();

        if (guid == null)
            throw new ArgumentException(Strings.General.MissingGuidAttribute(owningPackage.GetType()));

        return new Guid(guid.Value);
    }

    static Guid GetPackageGuidOrThrow<TPackage>() where TPackage : IVsPackage
    {
        var guidString = typeof(TPackage)
               .GetCustomAttributes(true)
               .OfType<GuidAttribute>()
               .Select(g => g.Value)
               .FirstOrDefault();

        if (guidString == null)
            throw new ArgumentException(Strings.IServiceProviderExtensions.MissingGuidAttribute(typeof(TPackage)));

        return new Guid(guidString);
    }

    /// <summary>
    /// Retrieves an existing loaded package or loads it 
    /// automatically if needed.
    /// </summary>
    /// <typeparam name="TPackage">The type of the package to load.</typeparam>
    /// <returns>The fully loaded and initialized package.</returns>
    public static TPackage GetLoadedPackage<TPackage>(this IServiceProvider serviceProvider) where TPackage : IVsPackage =>
        (TPackage)serviceProvider.GetLoadedPackage(GetPackageGuidOrThrow<TPackage>());

    /// <summary>
    /// Retrieves an existing loaded package or loads it 
    /// automatically if needed.
    /// </summary>
    /// <typeparam name="TPackage">The type of the package to load.</typeparam>
    /// <returns>The fully loaded and initialized package.</returns>
    public async static System.Threading.Tasks.Task<TPackage> GetLoadedPackageAsync<TPackage>(this IServiceProvider serviceProvider) where TPackage : IVsPackage =>
        (TPackage)await serviceProvider.GetLoadedPackageAsync(GetPackageGuidOrThrow<TPackage>());

    /// <summary>
    /// Retrieves an existing loaded package or loads it 
    /// automatically if needed.
    /// </summary>
    /// <returns>The fully loaded and initialized package.</returns>
    public static IServiceProvider GetLoadedPackage(this IServiceProvider serviceProvider, Guid packageId)
    {
        var jtf = GetJTF(serviceProvider);

        return jtf.Run(async () =>
        {
            await jtf.SwitchToMainThreadAsync();

            var vsShell = serviceProvider.GetService<SVsShell, IVsShell>();
            vsShell.IsPackageLoaded(ref packageId, out var vsPackage);

            if (vsPackage == null)
                ErrorHandler.ThrowOnFailure(vsShell.LoadPackage(ref packageId, out vsPackage));

            return (IServiceProvider)vsPackage;
        });
    }

    /// <summary>
    /// Retrieves an existing loaded package or loads it 
    /// automatically if needed.
    /// </summary>
    /// <returns>The fully loaded and initialized package.</returns>
    public async static System.Threading.Tasks.Task<IServiceProvider> GetLoadedPackageAsync(this IServiceProvider serviceProvider, Guid packageId)
    {
        var jtf = GetJTF(serviceProvider);

        await jtf.SwitchToMainThreadAsync();

        var vsShell = serviceProvider.GetService<SVsShell, IVsShell>();
        vsShell.IsPackageLoaded(ref packageId, out var vsPackage);

        if (vsPackage == null)
        {
            await (vsShell as IVsShell7)?.LoadPackageAsync(ref packageId);

            vsShell.IsPackageLoaded(ref packageId, out vsPackage);
        }

        return (IServiceProvider)vsPackage;
    }

    static JoinableTaskFactory GetJTF(IServiceProvider serviceProvider) =>
        serviceProvider
            .GetService<SComponentModel, IComponentModel>()
            .GetService<JoinableTaskContext>()
            .Factory;
}