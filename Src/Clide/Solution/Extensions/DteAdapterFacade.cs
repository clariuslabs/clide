namespace EnvDTE
{
	using Clide.Patterns.Adapter;
	using Clide.Solution;
	using Microsoft.VisualStudio.Shell.Interop;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using VSLangProj;

	/// <summary>
	/// Facades are created in the namespace of the source type namespace
	/// for easy discoverability. They expose the available conversions from 
	/// a given type to all the supported target types.
	/// </summary>
	public static class AdapterFacade
	{
		#region Solution

		/// <summary>
		/// Adapts the specified solution to supported target types.
		/// </summary>
		/// <param name="solution">The solution to adapt.</param>
		/// <returns>The entry point that exposes supported target types.</returns>
		public static IAdaptable<Solution> Adapt(this Solution solution)
		{
			return new Adaptable<Solution>(Adapters.ServiceInstance, solution);
		}

		/// <summary>
		/// Adapts a <see cref="Solution"/> to an <see cref="ISolutionNode"/>.
		/// </summary>
		/// <returns>The <see cref="ISolutionNode"/> or <see langword="null"/> if conversion is not possible.</returns>
		public static ISolutionNode AsSolutionNode(this IAdaptable<Solution> adaptable)
		{
			return adaptable.As<ISolutionNode>();
		}

		/// <summary>
		/// Adapts a <see cref="Solution"/> to an <see cref="IVsSolution"/>.
		/// </summary>
		/// <returns>The <see cref="IVsSolution"/> or <see langword="null"/> if conversion is not possible.</returns>
		public static IVsSolution AsVsSolution(this IAdaptable<Solution> adaptable)
		{
			return adaptable.AsSolutionNode().As<IVsSolution>();
		}

		#endregion

		#region Project

		/// <summary>
		/// Adapts the specified project to supported target types.
		/// </summary>
		/// <param name="project">The project to adapt.</param>
		/// <returns>The entry point that exposes supported target types.</returns>
		public static IAdaptable<Project> Adapt(this Project project)
		{
			return new Adaptable<Project>(Adapters.ServiceInstance, project);
		}

		/// <summary>
		/// Adapts a <see cref="Project"/> to an <see cref="IProjectNode"/>.
		/// </summary>
		/// <returns>The <see cref="IProjectNode"/> or <see langword="null"/> if conversion is not possible.</returns>
		public static IProjectNode AsProjectNode(this IAdaptable<Project> adaptable)
		{
			return adaptable.As<IProjectNode>();
		}

		/// <summary>
		/// Adapts a <see cref="Project"/> to an <see cref="IVsProject"/>.
		/// </summary>
		/// <returns>The <see cref="IVsProject"/> or <see langword="null"/> if conversion is not possible.</returns>
		public static IVsProject AsVsProject(this IAdaptable<Project> adaptable)
		{
			return adaptable.AsProjectNode().As<IVsProject>();
		}

		/// <summary>
		/// Adapts a <see cref="Project"/> to a <see cref="Microsoft.Build.Evaluation.Project"/>.
		/// </summary>
		/// <returns>The <see cref="Microsoft.Build.Evaluation.Project"/> or <see langword="null"/> if conversion is not possible.</returns>
		public static Microsoft.Build.Evaluation.Project AsMsBuildProject(this IAdaptable<Project> adaptable)
		{
			return adaptable.AsProjectNode().As<Microsoft.Build.Evaluation.Project>();
		}

		/// <summary>
		/// Adapts a <see cref="Project"/> to a <see cref="VSProject"/>.
		/// </summary>
		/// <returns>The <see cref="VSProject"/> or <see langword="null"/> if conversion is not possible.</returns>
		public static VSProject AsVsLangProject(this IAdaptable<Project> adaptable)
		{
			return adaptable.AsProjectNode().As<VSProject>();
		}

		#endregion

		#region ProjectItem

		/// <summary>
		/// Adapts the specified item to supported target types.
		/// </summary>
		/// <param name="item">The item to adapt.</param>
		/// <returns>The entry point that exposes supported target types.</returns>
		public static IAdaptable<ProjectItem> Adapt(this ProjectItem item)
		{
			return new Adaptable<ProjectItem>(Adapters.ServiceInstance, item);
		}

		/// <summary>
		/// Adapts a <see cref="ProjectItem"/> to an <see cref="IItemNode"/>.
		/// </summary>
		/// <returns>The <see cref="IItemNode"/> or <see langword="null"/> if conversion is not possible.</returns>
		public static IItemNode AsItemNode(this IAdaptable<ProjectItem> adaptable)
		{
			return adaptable.As<IItemNode>();
		}

		/// <summary>
		/// Adapts a <see cref="ProjectItem"/> to a <see cref="Microsoft.Build.Evaluation.ProjectItem"/>.
		/// </summary>
		/// <returns>The <see cref="Microsoft.Build.Evaluation.ProjectItem"/> or <see langword="null"/> if conversion is not possible.</returns>
		public static Microsoft.Build.Evaluation.ProjectItem AsMsBuildItem(this IAdaptable<ProjectItem> adaptable)
		{
			return adaptable.AsItemNode().As<Microsoft.Build.Evaluation.ProjectItem>();
		}

		/// <summary>
		/// Adapts a <see cref="ProjectItem"/> to an <see cref="VSProjectItem"/>.
		/// </summary>
		/// <returns>The <see cref="VSProjectItem"/> or <see langword="null"/> if conversion is not possible.</returns>
		public static VSProjectItem AsVsLangItem(this IAdaptable<ProjectItem> adaptable)
		{
			return adaptable.AsItemNode().As<VSProjectItem>();
		}

		#endregion

		#region References

		/// <summary>
		/// Adapts the specified reference to supported target types.
		/// </summary>
		/// <param name="reference">The reference to adapt.</param>
		/// <returns>The entry point that exposes supported target types.</returns>
		public static IAdaptable<Reference> Adapt(this Reference reference)
		{
			return new Adaptable<Reference>(Adapters.ServiceInstance, reference);
		}

		/// <summary>
		/// Adapts a <see cref="Reference"/> to an <see cref="IReferenceNode"/>.
		/// </summary>
		/// <returns>The <see cref="IReferenceNode"/> or <see langword="null"/> if conversion is not possible.</returns>
		public static IReferenceNode AsReferenceNode(this IAdaptable<Reference> adaptable)
		{
			return adaptable.As<IReferenceNode>();
		}

		/// <summary>
		/// Adapts the specified references to supported target types.
		/// </summary>
		/// <param name="references">The reference to adapt.</param>
		/// <returns>The entry point that exposes supported target types.</returns>
		public static IAdaptable<References> Adapt(this References references)
		{
			return new Adaptable<References>(Adapters.ServiceInstance, references);
		}

		/// <summary>
		/// Adapts a <see cref="References"/> to an <see cref="IReferencesNode"/>.
		/// </summary>
		/// <returns>The <see cref="IReferencesNode"/> or <see langword="null"/> if conversion is not possible.</returns>
		public static IReferencesNode AsItemNode(this IAdaptable<References> adaptable)
		{
			return adaptable.As<IReferencesNode>();
		}

		#endregion
	}
}
