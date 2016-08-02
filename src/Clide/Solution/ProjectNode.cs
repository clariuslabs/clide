using System;
using System.IO;
using System.Linq;
using Clide.Properties;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VSLangProj;

namespace Clide
{
	/// <summary>
	/// Default implementation of a managed project.
	/// </summary>
	public class ProjectNode : SolutionExplorerNode, IProjectNode
	{
		const string SharedProjectExtension = ".shproj";

		Lazy<GlobalProjectProperties> properties;

		/// <summary>
		/// Initializes a new instance of the <see cref="ProjectNode"/> class.
		/// </summary>
		/// <param name="hierarchyNode">The underlying hierarchy represented by this node.</param>
		/// <param name="nodeFactory">The factory for child nodes.</param>
		/// <param name="adapter">The adapter service that implements the smart cast <see cref="ITreeNode.As{T}"/>.</param>
		public ProjectNode(
			IVsHierarchyItem hierarchyNode,
			ISolutionExplorerNodeFactory nodeFactory,
			IAdapterService adapter,
			Lazy<IVsUIHierarchyWindow> solutionExplorer)
			: base(SolutionNodeKind.Project, hierarchyNode, nodeFactory, adapter, solutionExplorer)
		{
			properties = new Lazy<GlobalProjectProperties>(() => new GlobalProjectProperties(this));
		}

		/// <summary>
		/// Saves pending changes to the project file.
		/// </summary>
		public virtual void Save()
		{
			ErrorHandler.ThrowOnFailure(HierarchyNode
				.GetServiceProvider()
				.GetService<SVsSolution, IVsSolution>()
				.SaveSolutionElement(
					(uint)__VSSLNSAVEOPTIONS.SLNSAVEOPT_ForceSave,
					HierarchyNode.HierarchyIdentity.Hierarchy,
					0));
		}

		/// <summary>
		/// Gets the logical path of the project, relative to the solution, 
		/// considering any containing solution folders.
		/// </summary>
		public virtual string LogicalPath => this.RelativePathTo(OwningSolution);

		/// <summary>
		/// Gets the physical path of the project if it's file-based. 
		/// Returns <see langword="null"/> otherwise.
		/// </summary>
		public virtual string PhysicalPath
		{
			get
			{
				var project = HierarchyNode.GetActualHierarchy() as IVsProject;
				string filePath;
				if (project != null && ErrorHandler.Succeeded(project.GetMkDocument(HierarchyNode.GetActualItemId(), out filePath)) &&
					File.Exists(filePath))
					return filePath;

				return null;
			}
		}

		/// <summary>
		/// Gets the global properties of the project.
		/// </summary>
		/// <remarks>
		/// The default implementation for managed projects aggregates the 
		/// DTE properties and the MSBuild properties for the project. When 
		/// setting these properties, if an existing DTE property exists, 
		/// it's set, otherwise, an MSBuild property set is performed.
		/// </remarks>
		public virtual dynamic Properties => properties.Value;

		/// <summary>
		/// Gets the configuration-specific properties for the project.
		/// </summary>
		/// <param name="configurationAndPlatform">Configuration names are the combination 
		/// of a project configuration and the platform, like "Debug|AnyCPU".</param>
		/// <remarks>
		/// To set properties for the current project configuration only, use 
		/// <c>project.PropertiesFor(project.Configuration.ActiveConfigurationName)</c>.
		/// </remarks>
		public virtual dynamic PropertiesFor(string configurationAndPlatform) => new ConfigProjectProperties(this, configurationAndPlatform);

		/// <summary>
		/// Gets the user-specific properties of the project.
		/// </summary>
		/// <exception cref="System.NotImplementedException"></exception>
		public dynamic UserProperties => new UserProjectProperties(this);

		/// <summary>
		/// Gets the configuration-specific user properties for the project.
		/// </summary>
		/// <param name="configurationName">Configuration names are the combination
		/// of a project configuration and the platform, like "Debug|AnyCPU".</param>
		/// <returns></returns>
		/// <exception cref="System.NotImplementedException"></exception>
		/// <remarks>
		/// To set properties for the current project configuration only, use
		/// <c>project.UserPropertiesFor(project.Configuration.ActiveConfigurationName)</c>.
		/// </remarks>
		public dynamic UserPropertiesFor(string configurationName) => new ConfigUserProjectProperties(this, configurationName);

		/// <summary>
		/// Accepts the specified visitor for traversal.
		/// </summary>
		public override bool Accept(ISolutionVisitor visitor) => SolutionVisitable.Accept(this, visitor);

		/// <summary>
		/// Tries to smart-cast this node to the give type.
		/// </summary>
		/// <typeparam name="T">Type to smart-cast to.</typeparam>
		/// <returns>
		/// The casted value or null if it cannot be converted to that type.
		/// </returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public override T As<T>() => Adapter.Adapt(this).As<T>();

		public bool IsSharedProject =>
			SharedProjectExtension.Equals(
				Path.GetExtension(PhysicalPath),
				StringComparison.InvariantCultureIgnoreCase);
	}
}