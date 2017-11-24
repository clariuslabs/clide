namespace System
{
	using Clide;
	using Clide.Properties;
	using Microsoft.VisualStudio;
	using Microsoft.VisualStudio.Shell.Interop;
	using System.ComponentModel;
	using System.Linq;
	using System.Runtime.InteropServices;
	using Microsoft.VisualStudio.Shell;
	using Merq;
	using Microsoft.VisualStudio.ComponentModelHost;

	/// <summary>
	/// Provides useful extensions to the IDE service provider.
	/// </summary>
	public static partial class ServiceProviderExtensions
	{
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

		/// <summary>
		/// Retrieves an existing loaded package or loads it 
		/// automatically if needed.
		/// </summary>
		/// <typeparam name="TPackage">The type of the package to load.</typeparam>
		/// <returns>The fully loaded and initialized package.</returns>
		public static TPackage GetLoadedPackage<TPackage>(this IServiceProvider serviceProvider)
		{
			var asyncManager = GetAsyncManager(serviceProvider);

			return asyncManager.Run(async () =>
			{
				await asyncManager.SwitchToMainThread();

				var guidString = typeof(TPackage)
					.GetCustomAttributes(true)
					.OfType<GuidAttribute>()
					.Select(g => g.Value)
					.FirstOrDefault();

				if (guidString == null)
					throw new ArgumentException(Strings.IServiceProviderExtensions.MissingGuidAttribute(typeof(TPackage)));

				var guid = new Guid(guidString);
				var vsPackage = default(IVsPackage);

				var vsShell = serviceProvider.GetService<SVsShell, IVsShell>();
				vsShell.IsPackageLoaded(ref guid, out vsPackage);

				if (vsPackage == null)
					ErrorHandler.ThrowOnFailure(vsShell.LoadPackage(ref guid, out vsPackage));

				return (TPackage)vsPackage;
			});
		}

		/// <summary>
		/// Retrieves an existing loaded package or loads it 
		/// automatically if needed.
		/// </summary>
		/// <returns>The fully loaded and initialized package.</returns>
		public static IServiceProvider GetLoadedPackage(this IServiceProvider serviceProvider, Guid packageId)
		{
			var asyncManager = GetAsyncManager(serviceProvider);

			return asyncManager.Run(async () =>
			{
				await asyncManager.SwitchToMainThread();

				var vsPackage = default(IVsPackage);

				var vsShell = serviceProvider.GetService<SVsShell, IVsShell>();
				vsShell.IsPackageLoaded(ref packageId, out vsPackage);

				if (vsPackage == null)
					ErrorHandler.ThrowOnFailure(vsShell.LoadPackage(ref packageId, out vsPackage));

				return (IServiceProvider)vsPackage;
			});
		}

		static IAsyncManager GetAsyncManager(IServiceProvider serviceProvider) =>
			serviceProvider
				.GetService<SComponentModel, IComponentModel>()
				.GetService<IAsyncManager>();
	}
}
