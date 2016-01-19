using System;
using System.Collections.Generic;
using System.ComponentModel;
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
[EditorBrowsable (EditorBrowsableState.Never)]
public static partial class Adapters
{
	/// <summary>
	/// Adapts a <see cref="ISolutionNode"/> to a DTE <see cref="Solution"/>.
	/// </summary>
	/// <returns>The DTE <see cref="Solution"/> or <see langword="null"/> if conversion is not possible.</returns>
	public static Solution AsSolution (this ISolutionNode solution) => solution.As<EnvDTE.Solution> ();

	/// <summary>
	/// Adapts a <see cref="ISolutionNode"/> to an <see cref="IVsSolution"/>.
	/// </summary>
	/// <returns>The <see cref="IVsSolution"/> or <see langword="null"/> if conversion is not possible.</returns>
	public static IVsSolution AsVsSolution (this ISolutionNode solution) => solution.As<IVsSolution> ();

	/// <summary>
	/// Adapts a <see cref="IProjectNode"/> to an <see cref="IVsProject"/>.
	/// </summary>
	/// <returns>The <see cref="IVsProject"/> or <see langword="null"/> if conversion is not possible.</returns>
	public static IVsProject AsVsProject (this IProjectNode project) => project.As<IVsProject> ();

	/// <summary>
	/// Adapts a <see cref="IProjectNode"/> to a <see cref="VSProject"/>.
	/// </summary>
	/// <returns>The <see cref="VSProject"/> or <see langword="null"/> if conversion is not possible.</returns>
	public static VSProject AsVsLangProject (this IProjectNode project) => project.As<VSProject> ();

	/// <summary>
	/// Adapts a <see cref="IItemNode"/> to a <see cref="ProjectItem"/>.
	/// </summary>
	/// <returns>The <see cref="ProjectItem"/> or <see langword="null"/> if conversion is not possible.</returns>
	public static ProjectItem AsProjectItem (this IItemNode item) => item.As<ProjectItem> ();

	/// <summary>
	/// Adapts a <see cref="IItemNode"/> to an <see cref="VSProjectItem"/>.
	/// </summary>
	/// <returns>The <see cref="VSProjectItem"/> or <see langword="null"/> if conversion is not possible.</returns>
	public static VSProjectItem AsVsLangProjectItem (this IItemNode item) => item.As<VSProjectItem> ();

	/// <summary>
	/// Adapts a <see cref="IReferenceNode"/> to a VsLang <see cref="Reference"/>.
	/// </summary>
	/// <returns>The <see cref="Reference"/> or <see langword="null"/> if conversion is not possible.</returns>
	public static Reference AsReference (this IReferenceNode reference) => reference.As<Reference> ();

	/// <summary>
	/// Adapts a <see cref="IReferencesNode"/> to a VsLang <see cref="References"/>.
	/// </summary>
	/// <returns>The <see cref="References"/> or <see langword="null"/> if conversion is not possible.</returns>
	public static References AsReferences (this IReferencesNode references) => references.As<References> ();

	/*
	/// <summary>
	/// Adapts a <see cref="IProjectNode"/> to a <see cref="Microsoft.Build.Evaluation.Project"/>.
	/// </summary>
	/// <returns>The <see cref="Microsoft.Build.Evaluation.Project"/> or <see langword="null"/> if conversion is not possible.</returns>
	public static Microsoft.Build.Evaluation.Project AsMsBuildProject (this IAdaptable<IProjectNode> adaptable)
	{
		return adaptable.As<Microsoft.Build.Evaluation.Project> ();
	}


	/// <summary>
	/// Adapts a <see cref="IItemNode"/> to a <see cref="Microsoft.Build.Evaluation.ProjectItem"/>.
	/// </summary>
	/// <returns>The <see cref="Microsoft.Build.Evaluation.ProjectItem"/> or <see langword="null"/> if conversion is not possible.</returns>
	public static Microsoft.Build.Evaluation.ProjectItem AsMsBuildItem (this IAdaptable<IItemNode> adaptable)
	{
		return adaptable.As<Microsoft.Build.Evaluation.ProjectItem> ();
	}
	*/
}