namespace Microsoft.Build.Evaluation
{
	using Clide.Patterns.Adapter;
	using Clide.Solution;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using VSLangProj;
	using Microsoft.VisualStudio.Shell.Interop;

	/// <summary>
	/// Facades are created in the namespace of the source type namespace
	/// for easy discoverability. They expose the available conversions from 
	/// a given type to all the supported target types.
	/// </summary>
	public static class AdapterFacade
	{
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
		/// Adapts a <see cref="Project"/> to a DTE <see cref="EnvDTE.Project"/>.
		/// </summary>
		/// <returns>The <see cref="Project"/> or <see langword="null"/> if conversion is not possible.</returns>
		public static EnvDTE.Project AsDteProject(this IAdaptable<Project> adaptable)
		{
			return adaptable.AsProjectNode().As<EnvDTE.Project>();
		}

		/// <summary>
		/// Adapts a <see cref="Project"/> to a <see cref="IVsProject"/>.
		/// </summary>
		/// <returns>The <see cref="IVsProject"/> or <see langword="null"/> if conversion is not possible.</returns>
		public static IVsProject AsVsProject(this IAdaptable<Project> adaptable)
		{
			return adaptable.AsProjectNode().As<IVsProject>();
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
		/// Adapts a <see cref="ProjectItem"/> to a <see cref="EnvDTE.ProjectItem"/>.
		/// </summary>
		/// <returns>The <see cref="EnvDTE.ProjectItem"/> or <see langword="null"/> if conversion is not possible.</returns>
		public static EnvDTE.ProjectItem AsDteProjectItem(this IAdaptable<ProjectItem> adaptable)
		{
			return adaptable.AsItemNode().As<EnvDTE.ProjectItem>();
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
	}
}
