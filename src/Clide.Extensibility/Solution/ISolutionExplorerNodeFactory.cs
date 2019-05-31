using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

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
        ISolutionExplorerNode CreateNode(IVsHierarchyItem item);

        /// <summary>
        /// Creates the node for the given hierarchy
        /// </summary>
        ISolutionExplorerNode CreateNode(IVsHierarchy hierarchy, uint itemId = VSConstants.VSITEMID_ROOT);
    }
}