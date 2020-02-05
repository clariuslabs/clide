namespace Clide
{
    /// <summary>
    /// Represents a project-level node that contains project items.
    /// </summary>
    public interface IProjectItemContainerNode
    {
        /// <summary>
        /// Creates a nested folder.
        /// </summary>
        /// <param name="name">The name of the folder to create.</param>
        IFolderNode CreateFolder(string name);

        IItemNode AddItem(string path);
    }
}
