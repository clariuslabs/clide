using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using Clide;
using Clide.Properties;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell.Interop;
using Ole = Microsoft.VisualStudio.OLE.Interop;

/// <summary>
/// Provides extension methods to retrieve a service locator 
/// from various Visual Studio primitive types.
/// </summary>
[EditorBrowsable (EditorBrowsableState.Never)]
public static class ServiceLocatorFacades
{
	/// <summary>
	/// Retrieves the <see cref="IServiceLocator"/> for the given service provider.
	/// </summary>
	/// <exception cref="ArgumentNullException">The <paramref name="services"/> parameter was null.</exception>
	/// <exception cref="InvalidOperationException">The required <see cref="IComponentModel"/> service was not found.</exception>
	public static IServiceLocator GetServiceLocator (this IServiceProvider services)
	{
		Guard.NotNull (nameof (services), services);

		var components = services.GetService<SComponentModel, IComponentModel>();

		try {
			return components.GetService<IServiceLocatorProvider> ().GetServiceLocator (services);
		} catch (ImportCardinalityMismatchException ex) {
			throw new MissingDependencyException (Strings.ServiceLocator.MissingDependency (typeof (IServiceLocatorProvider)), ex);
		}
	}

	/// <summary>
	/// Retrieves the <see cref="IServiceLocator"/> for the given <see cref="DTE"/> instance.
	/// </summary>
	/// <exception cref="ArgumentNullException">The <paramref name="dte"/> parameter was null.</exception>
	/// <exception cref="InvalidOperationException">The required <see cref="IComponentModel"/> service was not found.</exception>
	public static IServiceLocator GetServiceLocator (this DTE dte)
	{
		Guard.NotNull (nameof (dte), dte);

		var components = new OleServiceProvider(dte).GetService<SComponentModel, IComponentModel>();

		try {
			return components.GetService<IServiceLocatorProvider> ().GetServiceLocator (dte);
		} catch (ImportCardinalityMismatchException ex) {
			throw new MissingDependencyException (Strings.ServiceLocator.MissingDependency (typeof (IServiceLocatorProvider)), ex);
		}
	}

	/// <summary>
	/// Retrieves the <see cref="IServiceLocator"/> for the given <see cref="Solution"/>.
	/// </summary>
	/// <exception cref="ArgumentNullException">The <paramref name="solution"/> parameter was null.</exception>
	/// <exception cref="InvalidOperationException">The required <see cref="IComponentModel"/> service was not found.</exception>
	public static IServiceLocator GetServiceLocator (this Solution solution)
	{
		Guard.NotNull (nameof (solution), solution);

		return solution.DTE.GetServiceLocator ();
	}


	/// <summary>
	/// Retrieves the <see cref="IServiceLocator"/> for the given project.
	/// </summary>
	/// <exception cref="ArgumentNullException">The <paramref name="project"/> parameter was null.</exception>
	/// <exception cref="InvalidOperationException">The required <see cref="IComponentModel"/> service was not found.</exception>
	public static IServiceLocator GetServiceLocator (this Project project)
	{
		Guard.NotNull (nameof (project), project);

		var components = new OleServiceProvider(project.DTE).GetService<SComponentModel, IComponentModel>();
		try {
			return components.GetService<IServiceLocatorProvider> ().GetServiceLocator (project);
		} catch (ImportCardinalityMismatchException ex) {
			throw new MissingDependencyException (Strings.ServiceLocator.MissingDependency (typeof (IServiceLocatorProvider)), ex);
		}
	}

	/// <summary>
	/// Retrieves the <see cref="IServiceLocator"/> for the given hierarchy.
	/// </summary>
	/// <exception cref="ArgumentNullException">The <paramref name="hierarchy"/> parameter was null.</exception>
	/// <exception cref="InvalidOperationException">The required <see cref="IComponentModel"/> service was not found.</exception>
	public static IServiceLocator GetServiceLocator (this IVsHierarchy hierarchy)
	{
		Guard.NotNull (nameof (hierarchy), hierarchy);

		IServiceProvider services;
		Ole.IServiceProvider site;
		if (ErrorHandler.Failed (hierarchy.GetSite (out site)))
			services = Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider;
		else
			services = new OleServiceProvider (site);

		var components = services.GetService<SComponentModel, IComponentModel>();
		try {
			return components.GetService<IServiceLocatorProvider> ().GetServiceLocator (hierarchy);
		} catch (ImportCardinalityMismatchException ex) {
			throw new MissingDependencyException (Strings.ServiceLocator.MissingDependency (typeof (IServiceLocatorProvider)), ex);
		}
	}

	/// <summary>
	/// Retrieves the <see cref="IServiceLocator"/> for the given project.
	/// </summary>
	/// <exception cref="ArgumentNullException">The <paramref name="project"/> parameter was null.</exception>
	/// <exception cref="InvalidOperationException">The required <see cref="IComponentModel"/> service was not found.</exception>
	public static IServiceLocator GetServiceLocator (this IVsProject project)
	{
		Guard.NotNull (nameof (project), project);

		IServiceProvider serviceProvider;
		Ole.IServiceProvider site;
		if (ErrorHandler.Failed (((IVsHierarchy)project).GetSite (out site)))
			serviceProvider = Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider;
		else
			serviceProvider = new OleServiceProvider (site);

		var components = serviceProvider.GetService<SComponentModel, IComponentModel>();

		try {
			return components.GetService<IServiceLocatorProvider> ().GetServiceLocator (project);
		} catch (ImportCardinalityMismatchException ex) {
			throw new MissingDependencyException (Strings.ServiceLocator.MissingDependency (typeof (IServiceLocatorProvider)), ex);
		}
	}
}
