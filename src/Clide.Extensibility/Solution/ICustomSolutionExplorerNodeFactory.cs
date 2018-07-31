using Microsoft.VisualStudio.Shell;

namespace Clide
{
    /// <summary>
    /// Creates solution nodes from hierarchy items.
    /// </summary>
    public interface ICustomSolutionExplorerNodeFactory
    {
        /// <summary>
        /// Whether this factory supports the hierarchy item.
        /// </summary>
        bool Supports(IVsHierarchyItem item);

        /// <summary>
        /// Creates the node for the given hierarchy item.
        /// </summary>
        ISolutionExplorerNode CreateNode(IVsHierarchyItem item);
    }
}
