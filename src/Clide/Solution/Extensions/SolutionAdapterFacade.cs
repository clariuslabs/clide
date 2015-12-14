namespace Clide
{
	using Clide.Patterns.Adapter;
	using Clide.Solution;
	using Microsoft.VisualStudio.Shell.Interop;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using VSLangProj;
	using EnvDTE;

	/// <summary>
	/// Facades are created in the namespace of the source type namespace
	/// for easy discoverability. They expose the available conversions from 
	/// a given type to all the supported target types.
	/// </summary>
	public static class AdapterFacade
	{
		#region ISolutionNode

		/// <summary>
		/// Adapts the specified solution to supported target types.
		/// </summary>
		/// <param name="solution">The solution to adapt.</param>
		/// <returns>The entry point that exposes supported target types.</returns>
		public static IAdaptable<ISolutionNode> Adapt(this ISolutionNode solution)
		{
			return new TreeNodeAdaptable<ISolutionNode>(solution);
		}

		/// <summary>
		/// Adapts a <see cref="ISolutionNode"/> to a DTE <see cref="EnvDTE.Solution"/>.
		/// </summary>
		/// <returns>The DTE <see cref="Solution"/> or <see langword="null"/> if conversion is not possible.</returns>
		public static EnvDTE.Solution AsSolutionNode(this IAdaptable<ISolutionNode> adaptable)
		{
			return adaptable.As<EnvDTE.Solution>();
		}

		/// <summary>
		/// Adapts a <see cref="ISolutionNode"/> to an <see cref="IVsSolution"/>.
		/// </summary>
		/// <returns>The <see cref="IVsSolution"/> or <see langword="null"/> if conversion is not possible.</returns>
		public static IVsSolution AsVsSolution(this IAdaptable<ISolutionNode> adaptable)
		{
			return adaptable.As<IVsSolution>();
		}

		#endregion

		#region IProjectNode

		/// <summary>
		/// Adapts the specified project to supported target types.
		/// </summary>
		/// <param name="project">The project to adapt.</param>
		/// <returns>The entry point that exposes supported target types.</returns>
		public static IAdaptable<IProjectNode> Adapt(this IProjectNode project)
		{
			return new TreeNodeAdaptable<IProjectNode>(project);
		}

		/// <summary>
		/// Adapts a <see cref="IProjectNode"/> to an <see cref="IVsProject"/>.
		/// </summary>
		/// <returns>The <see cref="IVsProject"/> or <see langword="null"/> if conversion is not possible.</returns>
		public static IVsProject AsVsProject(this IAdaptable<IProjectNode> adaptable)
		{
			return adaptable.As<IVsProject>();
		}

		/// <summary>
		/// Adapts a <see cref="IProjectNode"/> to a <see cref="Microsoft.Build.Evaluation.Project"/>.
		/// </summary>
		/// <returns>The <see cref="Microsoft.Build.Evaluation.Project"/> or <see langword="null"/> if conversion is not possible.</returns>
		public static Microsoft.Build.Evaluation.Project AsMsBuildProject(this IAdaptable<IProjectNode> adaptable)
		{
			return adaptable.As<Microsoft.Build.Evaluation.Project>();
		}

		/// <summary>
		/// Adapts a <see cref="IProjectNode"/> to a <see cref="VSProject"/>.
		/// </summary>
		/// <returns>The <see cref="VSProject"/> or <see langword="null"/> if conversion is not possible.</returns>
		public static VSProject AsVsLangProject(this IAdaptable<IProjectNode> adaptable)
		{
			return adaptable.As<VSProject>();
		}

		#endregion

		#region IItemNode

		/// <summary>
		/// Adapts the specified item to supported target types.
		/// </summary>
		/// <param name="item">The item to adapt.</param>
		/// <returns>The entry point that exposes supported target types.</returns>
		public static IAdaptable<IItemNode> Adapt(this IItemNode item)
		{
			return new TreeNodeAdaptable<IItemNode>(item);
		}

		/// <summary>
		/// Adapts a <see cref="IItemNode"/> to a <see cref="Microsoft.Build.Evaluation.ProjectItem"/>.
		/// </summary>
		/// <returns>The <see cref="Microsoft.Build.Evaluation.ProjectItem"/> or <see langword="null"/> if conversion is not possible.</returns>
		public static Microsoft.Build.Evaluation.ProjectItem AsMsBuildItem(this IAdaptable<IItemNode> adaptable)
		{
			return adaptable.As<Microsoft.Build.Evaluation.ProjectItem>();
		}

		/// <summary>
		/// Adapts a <see cref="IItemNode"/> to an <see cref="VSProjectItem"/>.
		/// </summary>
		/// <returns>The <see cref="VSProjectItem"/> or <see langword="null"/> if conversion is not possible.</returns>
		public static VSProjectItem AsVsLangItem(this IAdaptable<IItemNode> adaptable)
		{
			return adaptable.As<VSProjectItem>();
		}

		#endregion

		#region IReferenceNode / IReferencesNode

		/// <summary>
		/// Adapts the specified reference to supported target types.
		/// </summary>
		/// <param name="reference">The reference to adapt.</param>
		/// <returns>The entry point that exposes supported target types.</returns>
		public static IAdaptable<IReferenceNode> Adapt(this IReferenceNode reference)
		{
			return new TreeNodeAdaptable<IReferenceNode>(reference);
		}

		/// <summary>
		/// Adapts a <see cref="IReferenceNode"/> to a VsLang <see cref="Reference"/>.
		/// </summary>
		/// <returns>The <see cref="Reference"/> or <see langword="null"/> if conversion is not possible.</returns>
		public static Reference AsReference(this IAdaptable<IReferenceNode> adaptable)
		{
			return adaptable.As<Reference>();
		}


		/// <summary>
		/// Adapts the specified references to supported target types.
		/// </summary>
		/// <param name="references">The reference to adapt.</param>
		/// <returns>The entry point that exposes supported target types.</returns>
		public static IAdaptable<IReferencesNode> Adapt(this IReferencesNode references)
		{
			return new TreeNodeAdaptable<IReferencesNode>(references);
		}

		/// <summary>
		/// Adapts a <see cref="IReferencesNode"/> to a VsLang <see cref="References"/>.
		/// </summary>
		/// <returns>The <see cref="References"/> or <see langword="null"/> if conversion is not possible.</returns>
		public static References AsReferences(this IAdaptable<IReferencesNode> adaptable)
		{
			return adaptable.As<References>();
		}

		#endregion

		private class TreeNodeAdaptable<TSource> : IAdaptable<TSource>
			where TSource : class, ITreeNode
		{
			private TSource source;

			/// <summary>
			/// Initializes a new instance of the <see cref="Adaptable{TSource}"/> class.
			/// </summary>
			/// <param name="source">The source object being adapted.</param>
			public TreeNodeAdaptable(TSource source)
			{
				this.source = source;
			}

			/// <summary>
			/// Adapts the instance to the given target type.
			/// </summary>
			/// <returns>The adapted instance or <see langword="null"/> if no compatible adapter was found.</returns>
			public T As<T>() where T : class
			{
				return source.As<T>();
			}
		}
	}
}
