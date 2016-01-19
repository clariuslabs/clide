using System;
using System.ComponentModel;
using Clide;
using Clide.Properties.Interfaces;

/// <summary>
/// Defines extension methods related to <see cref="IServiceProvider"/>.
/// </summary>
[EditorBrowsable (EditorBrowsableState.Never)]
public static partial class ServiceProviderExtensions
{
	/// <summary>
	/// Gets type-based services from the  service provider.
	/// </summary>
	/// <nuget id="netfx-System.ServiceProvider" />
	/// <typeparam name="T">The type of the service to get.</typeparam>
	/// <param name="provider" this="true">The service provider.</param>
	/// <returns>The requested service, or a <see langword="null"/> reference if the service could not be located.</returns>
	public static T TryGetService<T> (this IServiceProvider provider)
	{
		Guard.NotNull (nameof (provider), provider);

		return (T)provider.GetService (typeof (T));
	}

	/// <summary>
	/// Gets type-based services from the  service provider.
	/// </summary>
	/// <nuget id="netfx-System.ServiceProvider" />
	/// <typeparam name="T">The type of the service to get.</typeparam>
	/// <param name="provider" this="true">The service provider.</param>
	/// <returns>The requested service, or throws an <see cref="InvalidOperationException"/> 
	/// if the service was not found.</returns>
	public static T GetService<T> (this IServiceProvider provider)
	{
		Guard.NotNull (nameof (provider), provider);

		var service = (T)provider.GetService(typeof(T));
		if (service == null)
			throw new MissingDependencyException (Strings.ServiceLocator.MissingDependency (typeof (T)));

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
	public static TService TryGetService<TRegistration, TService> (this IServiceProvider provider)
	{
		Guard.NotNull (nameof (provider), provider);

		return (TService)provider.GetService (typeof (TRegistration));
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
	public static TService GetService<TRegistration, TService> (this IServiceProvider provider)
	{
		Guard.NotNull (nameof (provider), provider);

		var service = (TService)provider.GetService(typeof(TRegistration));
		if (service == null)
			throw new MissingDependencyException (Strings.ServiceLocator.MissingDependency (typeof (TRegistration)));

		return service;
	}
}