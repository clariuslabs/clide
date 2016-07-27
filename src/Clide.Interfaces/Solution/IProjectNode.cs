namespace Clide
{
	/// <summary>
	/// Represents a project in the solution explorer tree.
	/// </summary>
	public interface IProjectNode : ISolutionExplorerNode
	{
		///// <summary>
		///// Gets the project active configuration information.
		///// </summary>
		//IProjectConfiguration Configuration { get; }

		/// <summary>
		/// Gets the physical path of the project.
		/// </summary>
		string PhysicalPath { get; }

		/// <summary>
		/// Saves pending changes to the project file.
		/// </summary>
		void Save();

		/// <summary>
		/// Gets the global properties of the project.
		/// </summary>
		/// <remarks>
		/// The default implementation for managed projects aggregates the 
		/// DTE properties and the MSBuild properties for the project. When 
		/// setting these properties, if an existing DTE property exists, 
		/// it's set, otherwise, an MSBuild property set is performed.
		/// </remarks>
		dynamic Properties { get; }

		/// <summary>
		/// Gets the configuration-specific properties for the project.
		/// </summary>
		/// <param name="configurationName">Configuration names are the combination 
		/// of a project configuration and the platform, like "Debug|AnyCPU".</param>
		/// <remarks>
		/// To set properties for the current project configuration only, use 
		/// <c>project.PropertiesFor(project.Configuration.ActiveConfigurationName)</c>.
		/// </remarks>
		dynamic PropertiesFor(string configurationName);

		/// <summary>
		/// Gets the user-specific properties of the project.
		/// </summary>
		dynamic UserProperties { get; }

		/// <summary>
		/// Gets the configuration-specific user properties for the project.
		/// </summary>
		/// <param name="configurationName">Configuration names are the combination 
		/// of a project configuration and the platform, like "Debug|AnyCPU".</param>
		/// <remarks>
		/// To set properties for the current project configuration only, use 
		/// <c>project.UserPropertiesFor(project.Configuration.ActiveConfigurationName)</c>.
		/// </remarks>
		dynamic UserPropertiesFor(string configurationName);

		/// <summary>
		/// Returns true if it's a shared project
		/// </summary>
		bool IsSharedProject { get; }
	}
}
