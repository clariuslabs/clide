namespace Clide
{
	/// <summary>
	/// Interface implemented by solution item nodes.
	/// </summary>
	public interface ISolutionItemNode : ISolutionExplorerNode
	{
        /// <summary>
        /// Gets the owning solution folder.
        /// </summary>
        ISolutionFolderNode OwningSolutionFolder { get; }

        /// <summary>
        /// Gets the logical path of the item, relative to the solution, 
		/// considering any containing solution folders.
        /// </summary>
        string LogicalPath { get; }

        /// <summary>
        /// Gets the physical path of the solution item.
        /// </summary>
        string PhysicalPath { get; }
	}
}
