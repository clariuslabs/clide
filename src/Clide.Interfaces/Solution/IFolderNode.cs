namespace Clide
{
	/// <summary>
	/// Represents a folder inside a project or another folder.
	/// </summary>
	public interface IFolderNode : IProjectItemNode
	{
        /// <summary>
        /// Creates a nested folder.
        /// </summary>
        /// <param name="name">The name of the folder to create.</param>
        IFolderNode CreateFolder(string name);
	}
}