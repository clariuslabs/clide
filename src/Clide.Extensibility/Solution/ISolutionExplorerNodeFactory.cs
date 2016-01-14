using Microsoft.VisualStudio.Shell;

namespace Clide
{
	/// <summary>
	/// Creates solution nodes for hierarchy item, based on the registered and 
	/// supported <see cref="ICustomSolutionExplorerNodeFactory"/> components.
	/// </summary>
	public interface ISolutionExplorerNodeFactory
	{
		/// <summary>
		/// Creates the node for the given hierarchy item.
		/// </summary>
		ISolutionExplorerNode CreateNode (IVsHierarchyItem item);
	}
}