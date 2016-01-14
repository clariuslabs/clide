namespace Clide
{
	/// <summary>
	/// Interface implemented by the nodes that belong to a project.
	/// </summary>
	public interface IProjectItemNode : ISolutionExplorerNode
	{
		/// <summary>
		/// Gets the owning project.
		/// </summary>
		IProjectNode OwningProject { get; }
	}
}
