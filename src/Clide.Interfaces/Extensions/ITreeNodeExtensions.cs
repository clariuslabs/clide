using System.Collections.Generic;
using System.ComponentModel;
namespace Clide
{

    /// <summary>
    /// Provides node traversal extensions.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static class ITreeNodeExtensions
    {
        /// <summary>
        /// Traverses the specified node and all its descendents. The node itself 
        /// also exists in the returned enumeration. To traverse only the 
        /// descendents, traverse its <see cref="ITreeNode.Nodes"/> 
        /// property instead.
        /// </summary>
        /// <param name="node">The node to traverse.</param>
        /// <returns>The <paramref name="node"/> itself and all of its descendent nodes.</returns>
        public static IEnumerable<ISolutionExplorerNode> Traverse(this ISolutionExplorerNode node)
        {
            return new[] { node }.Traverse(TraverseKind.DepthFirst, x => x.Nodes);
        }

        /// <summary>
        /// Traverses the specified list of nodes and all their descendents. The nodes
        /// in the list are included in the returned enumeration.
        /// </summary>
        /// <param name="nodes">The nodes to traverse.</param>
        /// <returns>The <paramref name="nodes"/> themselves and all of their descendent nodes.</returns>
        public static IEnumerable<ISolutionExplorerNode> Traverse(this IEnumerable<ISolutionExplorerNode> nodes)
        {
            return nodes.Traverse(TraverseKind.DepthFirst, x => x.Nodes);
        }
    }
}