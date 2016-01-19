namespace Clide
{
	/// <summary>
	/// Represents an item inside a project, a project folder or a solution folder.
	/// </summary>
	public interface IItemNode : IProjectItemNode
	{
        /// <summary>
        /// Gets the logical path of the item, relative to its containing project.
        /// </summary>
        string LogicalPath { get; }

        /// <summary>
        /// Gets the physical path of the item.
        /// </summary>
        string PhysicalPath { get; }

        /// <summary>
        /// Gets the dynamic properties of the item.
        /// </summary>
        /// <remarks>
        /// The default implementation of item nodes exposes the 
        /// MSBuild item metadata properties using this property, 
        /// and allows getting and setting them.
        /// </remarks>
		dynamic Properties { get; }
	}
}
