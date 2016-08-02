namespace Clide
{
	/// <summary>
	/// Represents a node that can be deleted
	/// </summary>
	public interface IDeletableNode
	{
		/// <summary>
		/// Removes the node from its containing parent and also deleted the physical item
		/// </summary>
		void Delete();
	}
}