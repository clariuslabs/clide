using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clide;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using VSLangProj;

/// <summary>
/// Facades provide easy discoverability of available adapters, 
/// while still leveraging the <see cref="IServiceLocator"/> and 
/// <see cref="IServiceLocatorProvider"/> exported for the given 
/// target object.
/// </summary>
public static partial class AdapterFacade
{
	/// <summary>
	/// Adapts a <see cref="Reference"/> to an <see cref="IReferenceNode"/>.
	/// </summary>
	/// <returns>The <see cref="IReferenceNode"/> or <see langword="null"/> if conversion is not possible.</returns>
	public static IReferenceNode AsReferenceNode (this Reference reference) =>
		reference.DTE.GetServiceLocator ().GetExport<IAdapterService> ().Adapt (reference).As<IReferenceNode> ();

	/// <summary>
	/// Adapts a <see cref="References"/> to an <see cref="IReferencesNode"/>.
	/// </summary>
	/// <returns>The <see cref="IReferencesNode"/> or <see langword="null"/> if conversion is not possible.</returns>
	public static IReferencesNode AsItemNode (this References references) =>
		references.DTE.GetServiceLocator ().GetExport<IAdapterService> ().Adapt (references).As<IReferencesNode> ();
}
