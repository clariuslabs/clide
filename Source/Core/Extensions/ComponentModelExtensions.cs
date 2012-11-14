using System;
using System.Collections.Generic;
using System.Globalization;
using System.ComponentModel.Composition.Hosting;
using Microsoft.VisualStudio.ComponentModelHost;

/// <summary>
/// Defines extension methods related to <see cref="IServiceProvider"/> for use within Visual Studio.
/// </summary>
internal static class ComponentModelExtensions
{
	public static Lazy<T> GetExport<T>(this IServiceProvider provider)
	{
		return GetGlobalExportProviderOrThrow(provider).GetExport<T>();
	}

	public static Lazy<T, TMetadataView> GetExport<T, TMetadataView>(this IServiceProvider provider)
	{
		return GetGlobalExportProviderOrThrow(provider).GetExport<T, TMetadataView>();
	}

	public static Lazy<T> GetExport<T>(this IServiceProvider provider, string contractName)
	{
		return GetGlobalExportProviderOrThrow(provider).GetExport<T>(contractName);
	}

	public static Lazy<T, TMetadataView> GetExport<T, TMetadataView>(this IServiceProvider provider, string contractName)
	{
		return GetGlobalExportProviderOrThrow(provider).GetExport<T, TMetadataView>(contractName);
	}

	public static T GetExportedValue<T>(this IServiceProvider provider)
	{
		return GetGlobalExportProviderOrThrow(provider).GetExportedValue<T>();
	}

	public static T GetExportedValue<T>(this IServiceProvider provider, string contractName)
	{
		return GetGlobalExportProviderOrThrow(provider).GetExportedValue<T>(contractName);
	}

	public static T GetExportedValueOrDefault<T>(this IServiceProvider provider)
	{
		return GetGlobalExportProviderOrThrow(provider).GetExportedValueOrDefault<T>();
	}

	public static T GetExportedValueOrDefault<T>(this IServiceProvider provider, string contractName)
	{
		return GetGlobalExportProviderOrThrow(provider).GetExportedValueOrDefault<T>(contractName);
	}

	public static IEnumerable<T> GetExportedValues<T>(this IServiceProvider provider)
	{
		return GetGlobalExportProviderOrThrow(provider).GetExportedValues<T>();
	}

	public static IEnumerable<T> GetExportedValues<T>(this IServiceProvider provider, string contractName)
	{
		return GetGlobalExportProviderOrThrow(provider).GetExportedValues<T>(contractName);
	}

	public static IEnumerable<Lazy<T>> GetExports<T>(this IServiceProvider provider)
	{
		return GetGlobalExportProviderOrThrow(provider).GetExports<T>();
	}

	public static IEnumerable<Lazy<T, TMetadataView>> GetExports<T, TMetadataView>(this IServiceProvider provider)
	{
		return GetGlobalExportProviderOrThrow(provider).GetExports<T, TMetadataView>();
	}

	public static IEnumerable<Lazy<T>> GetExports<T>(this IServiceProvider provider, string contractName)
	{
		return GetGlobalExportProviderOrThrow(provider).GetExports<T>(contractName);
	}

	public static IEnumerable<Lazy<T, TMetadataView>> GetExports<T, TMetadataView>(this IServiceProvider provider, string contractName)
	{
		return GetGlobalExportProviderOrThrow(provider).GetExports<T, TMetadataView>(contractName);
	}

	private static ExportProvider GetGlobalExportProviderOrThrow(IServiceProvider provider)
	{
		var components = provider.GetService<SComponentModel, IComponentModel>();
		if (components == null)
		{
			throw new InvalidOperationException(string.Format(
				CultureInfo.CurrentCulture,
				"Service '{0}' is required.",
				typeof(ExportProvider)));
		}

		return components.DefaultExportProvider;
	}
}