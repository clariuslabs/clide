namespace Clide
{
	/// <summary>
	/// Represents a node that contains project references.
	/// </summary>
	public interface IReferenceContainerNode
	{
		/// <summary>
		/// Adds a project reference
		/// </summary>
		/// <param name="projectReference"></param>
		void AddReference(IProjectNode projectReference);
	}
}