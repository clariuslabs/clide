using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Clide;
using Clide.Properties.Interfaces;
using Ole = Microsoft.VisualStudio.OLE.Interop;

/// <summary>
/// Provides usability overloads to <see cref="IServiceLocator"/>.
/// </summary>
[EditorBrowsable (EditorBrowsableState.Never)]
public static class ServiceLocatorExtensions
{
	/// <summary>
	/// Get an instance of the given <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">Type of object requested.</typeparam>
	/// <param name="locator">The service locator to retrieve the instance from.</param>
	/// <param name="contractName">Optional name the object was registered with.</param>
	/// <exception cref="MissingDependencyException">if there is are errors resolving
	/// the service instance.</exception>
	/// <returns>The requested service instance.</returns>
	public static T TryGetExport<T> (this IServiceLocator locator, string contractName = null)
	{
		Guard.NotNull (nameof (locator), locator);

		try {
			return (T)locator.GetExport (typeof (T), contractName);
		} catch (MissingDependencyException) {
			return default (T);
		}
	}

	/// <summary>
	/// Get all instances of the given <typeparamref name="T"/> currently
	/// registered in the container.
	/// </summary>
	/// <typeparam name="T">Type of object requested.</typeparam>
	/// <typeparam name="TMetadata">The type of the metadata.</typeparam>
	/// <exception cref="MissingDependencyException">if there is are errors resolving
	/// the service instance.</exception>
	/// <returns>A sequence of instances of the requested <typeparamref name="T"/>.</returns>
	public static Lazy<T, TMetadata> TryGetExport<T, TMetadata> (this IServiceLocator locator, string contractName = null)
	{
		Guard.NotNull (nameof (locator), locator);

		try {
			var export = locator.GetExport (typeof (T), typeof (TMetadata), contractName);
			return new Lazy<T, TMetadata> (() => (T)export.Value, (TMetadata)export.Metadata);
		} catch (MissingDependencyException) {
			return null;
		}
	}

	/// <summary>
	/// Get an instance of the given <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">Type of object requested.</typeparam>
	/// <param name="locator">The service locator to retrieve the instance from.</param>
	/// <param name="contractName">Optional name the object was registered with.</param>
	/// <exception cref="MissingDependencyException">if there is are errors resolving
	/// the service instance.</exception>
	/// <returns>The requested service instance.</returns>
	public static T GetExport<T> (this IServiceLocator locator, string contractName = null)
	{
		Guard.NotNull (nameof (locator), locator);

		return (T)locator.GetExport (typeof (T), contractName);
	}

	/// <summary>
	/// Get all instances of the given <typeparamref name="T"/> currently
	/// registered in the container.
	/// </summary>
	/// <typeparam name="T">Type of object requested.</typeparam>
	/// <typeparam name="TMetadata">The type of the metadata.</typeparam>
	/// <exception cref="MissingDependencyException">if there is are errors resolving
	/// the service instance.</exception>
	/// <returns>A sequence of instances of the requested <typeparamref name="T"/>.</returns>
	public static Lazy<T, TMetadata> GetExport<T, TMetadata> (this IServiceLocator locator, string contractName = null)
	{
		Guard.NotNull (nameof (locator), locator);

		var export = locator.GetExport (typeof (T), typeof (TMetadata), contractName);
		return new Lazy<T, TMetadata> (() => (T)export.Value, (TMetadata)export.Metadata);
	}

	/// <summary>
	/// Get all instances of the given <typeparamref name="T"/> currently
	/// registered in the container.
	/// </summary>
	/// <typeparam name="T">Type of object requested.</typeparam>
	/// <exception cref="MissingDependencyException">if there is are errors resolving
	/// the service instance.</exception>
	/// <returns>A sequence of instances of the requested <typeparamref name="T"/>.</returns>
	public static IEnumerable<T> GetExports<T> (this IServiceLocator locator, string contractName = null)
	{
		Guard.NotNull (nameof (locator), locator);

		return locator.GetExports (typeof (T), contractName).Cast<T> ();
	}

	/// <summary>
	/// Get all instances of the given <typeparamref name="T"/> currently
	/// registered in the container.
	/// </summary>
	/// <typeparam name="T">Type of object requested.</typeparam>
	/// <typeparam name="TMetadata">The type of the metadata.</typeparam>
	/// <exception cref="MissingDependencyException">if there is are errors resolving
	/// the service instance.</exception>
	/// <returns>A sequence of instances of the requested <typeparamref name="T"/>.</returns>
	public static IEnumerable<Lazy<T, TMetadata>> GetExports<T, TMetadata> (this IServiceLocator locator, string contractName = null)
	{
		Guard.NotNull (nameof (locator), locator);

		return locator.GetExports (typeof (T), typeof (TMetadata), contractName)
			.Select (export => new Lazy<T, TMetadata> (() => (T)export.Value, (TMetadata)export.Metadata));
	}
}
