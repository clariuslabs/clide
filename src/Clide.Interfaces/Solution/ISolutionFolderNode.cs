namespace Clide
{
    /// <summary>
    /// Interface implemented by solution folder nodes.
    /// </summary>
    public interface ISolutionFolderNode : ISolutionExplorerNode
	{
        /// <summary>
        /// Creates a nested solution folder.
        /// </summary>
        /// <param name="name">The name of the folder to create.</param>
		ISolutionFolderNode CreateSolutionFolder(string name);
	}
}
