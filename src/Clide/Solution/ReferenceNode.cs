using System;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide
{
    /// <summary>
    /// Default implementation of a reference node in a managed project.
    /// </summary>
    public class ReferenceNode : ProjectItemNode, IReferenceNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceNode"/> class.
        /// </summary>
        /// <param name="hierarchyNode">The underlying hierarchy represented by this node.</param>
        /// <param name="nodeFactory">The factory for child nodes.</param>
        /// <param name="adapter">The adapter service that implements the smart cast <see cref="ITreeNode.As{T}"/>.</param>
        public ReferenceNode(
            IVsHierarchyItem hierarchyNode,
            ISolutionExplorerNodeFactory nodeFactory,
            IAdapterService adapter,
            JoinableLazy<IVsUIHierarchyWindow> solutionExplorer)
            : base(SolutionNodeKind.Reference, hierarchyNode, nodeFactory, adapter, solutionExplorer)
        {
        }

        /// <summary>
        /// Accepts the specified visitor for traversal.
        /// </summary>
        public override bool Accept(ISolutionVisitor visitor) => SolutionVisitable.Accept(this, visitor);

        /// <summary>
        /// Tries to smart-cast this node to the give type.
        /// </summary>
        /// <typeparam name="T">Type to smart-cast to.</typeparam>
        /// <returns>
        /// The casted value or null if it cannot be converted to that type.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override T As<T>() => Adapter.Adapt(this).As<T>();
    }
}