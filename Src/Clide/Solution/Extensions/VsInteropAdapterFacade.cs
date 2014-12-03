namespace Microsoft.VisualStudio.Shell.Interop
{
	using Clide.Patterns.Adapter;
	using Clide.Solution;
	using Clide.VisualStudio;
	using EnvDTE;
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
		#region IVsSolution

		/// <summary>
		/// Adapts the specified solution to supported target types.
		/// </summary>
		/// <param name="solution">The solution to adapt.</param>
		/// <returns>The entry point that exposes supported target types.</returns>
		public static IAdaptable<IVsSolution> Adapt(this IVsSolution solution)
		{
			return new Adaptable<IVsSolution>(Adapters.ServiceInstance, solution);
		}

		/// <summary>
		/// Adapts a <see cref="IVsSolution"/> to an <see cref="ISolutionNode"/>.
		/// </summary>
		/// <returns>The <see cref="ISolutionNode"/> or <see langword="null"/> if conversion is not possible.</returns>
		public static ISolutionNode AsSolutionNode(this IAdaptable<IVsSolution> adaptable)
		{
			return adaptable.As<ISolutionNode>();
		}

		/// <summary>
		/// Adapts a <see cref="IVsSolution"/> to a DTE <see cref="Solution"/>.
		/// </summary>
		/// <returns>The <see cref="Solution"/> or <see langword="null"/> if conversion is not possible.</returns>
		public static Solution AsDteSolution(this IAdaptable<IVsSolution> adaptable)
		{
			return adaptable.AsSolutionNode().As<Solution>();
		}

		#endregion

		#region IVsProject

		/// <summary>
		/// Adapts the specified project to supported target types.
		/// </summary>
		/// <param name="project">The project to adapt.</param>
		/// <returns>The entry point that exposes supported target types.</returns>
		public static IAdaptable<IVsProject> Adapt(this IVsProject project)
		{
			return new Adaptable<IVsProject>(Adapters.ServiceInstance, project);
		}

		/// <summary>
		/// Adapts a <see cref="IVsProject"/> to an <see cref="IProjectNode"/>.
		/// </summary>
		/// <returns>The <see cref="IProjectNode"/> or <see langword="null"/> if conversion is not possible.</returns>
		public static IProjectNode AsProjectNode(this IAdaptable<IVsProject> adaptable)
		{
			return adaptable.As<IProjectNode>();
		}

		/// <summary>
		/// Adapts a <see cref="IVsProject"/> to a DTE <see cref="Project"/>.
		/// </summary>
		/// <returns>The <see cref="Project"/> or <see langword="null"/> if conversion is not possible.</returns>
		public static Project AsDteProject(this IAdaptable<IVsProject> adaptable)
		{
			return adaptable.AsProjectNode().As<Project>();
		}

		/// <summary>
		/// Adapts a <see cref="IVsProject"/> to a <see cref="Microsoft.Build.Evaluation.Project"/>.
		/// </summary>
		/// <returns>The <see cref="Microsoft.Build.Evaluation.Project"/> or <see langword="null"/> if conversion is not possible.</returns>
		public static Microsoft.Build.Evaluation.Project AsMsBuildProject(this IAdaptable<IVsProject> adaptable)
		{
			return adaptable.AsProjectNode().As<Microsoft.Build.Evaluation.Project>();
		}

		/// <summary>
		/// Adapts a <see cref="IVsProject"/> to a <see cref="VSProject"/>.
		/// </summary>
		/// <returns>The <see cref="VSProject"/> or <see langword="null"/> if conversion is not possible.</returns>
		public static VSProject AsVsLangProject(this IAdaptable<IVsProject> adaptable)
		{
			return adaptable.AsProjectNode().As<VSProject>();
		}

		#endregion

		#region VsHierarchyItem

		/// <summary>
		/// Adapts the specified hierarchy item to supported target types.
		/// </summary>
		/// <param name="item">The hierarchy item to adapt.</param>
		/// <returns>The entry point that exposes supported target types.</returns>
		public static IAdaptable<VsHierarchyItem> Adapt(this VsHierarchyItem item)
		{
			return new Adaptable<VsHierarchyItem>(Adapters.ServiceInstance, item);
		}

		/// <summary>
		/// Adapts a <see cref="VsHierarchyItem"/> to an <see cref="ISolutionNode"/>.
		/// </summary>
		/// <returns>The <see cref="ISolutionNode"/> or <see langword="null"/> if conversion is not possible.</returns>
		public static ISolutionNode AsSolutionNode(this IAdaptable<VsHierarchyItem> adaptable)
		{
			return adaptable.As<ISolutionNode>();
		}

		/// <summary>
		/// Adapts a <see cref="VsHierarchyItem"/> to an <see cref="IProjectNode"/>.
		/// </summary>
		/// <returns>The <see cref="IProjectNode"/> or <see langword="null"/> if conversion is not possible.</returns>
		public static IProjectNode AsProjectNode(this IAdaptable<VsHierarchyItem> adaptable)
		{
			return adaptable.As<IProjectNode>();
		}

		/// <summary>
		/// Adapts a <see cref="VsHierarchyItem"/> to an <see cref="IItemNode"/>.
		/// </summary>
		/// <returns>The <see cref="IItemNode"/> or <see langword="null"/> if conversion is not possible.</returns>
		public static IItemNode AsItemNode(this IAdaptable<VsHierarchyItem> adaptable)
		{
			return adaptable.As<IItemNode>();
		}

		#endregion
	}
}
