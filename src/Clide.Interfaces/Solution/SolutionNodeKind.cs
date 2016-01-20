namespace Clide
{
	/// <summary>
	/// The kind of solution node.
	/// </summary>
	public enum SolutionNodeKind
	{
        /// <summary>
        /// The node is the solution node.
        /// </summary>
		Solution = 0,

        /// <summary>
        /// The node is a solution folder.
        /// </summary>
		SolutionFolder = 1,

        /// <summary>
        /// The node is a solution item, meaning it 
        /// exists in a solution folder, not  a project.
        /// </summary>
        SolutionItem = 2,

        /// <summary>
        /// The node is a project.
        /// </summary>
        Project = 3,

        /// <summary>
        /// The node is a project folder.
        /// </summary>
		Folder = 4,

        /// <summary>
        /// The node is a project item.
        /// </summary>
        Item = 5,

        /// <summary>
        /// The node is a reference in a project.
        /// </summary>
		Reference = 6,

        /// <summary>
        /// The node is the references folder.
        /// </summary>
		ReferencesFolder = 7,

        /// <summary>
        /// The node is of a generic or custom kind.
        /// </summary>
		Generic = 50,
	}
}
