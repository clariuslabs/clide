using System;
using Clide.Sdk;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
namespace Clide
{

    /// <summary>
    /// Represents a generic node that has no custom or fallback factory associated with it.
    /// Used internally by the <see cref="AggregateSolutionExplorerNodeFactory"/> to expose 
    /// generic traversal while remaining within ISolution nodes.
    /// </summary>
    class GenericNode : SolutionExplorerNode, IGenericNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericNode"/> class.
        /// </summary>
        /// <param name="hierarchyNode">The underlying hierarchy represented by this node.</param>
        /// <param name="parentNode">The parent node accessor.</param>
        /// <param name="nodeFactory">The factory for child nodes.</param>
        /// <param name="adapter">The adapter service that implements the smart cast <see cref="ISolutionExplorerNode.As{T}"/>.</param>
        public GenericNode(IVsHierarchyItem hierarchyNode,
            ISolutionExplorerNodeFactory nodeFactory,
            IAdapterService adapter,
            JoinableLazy<IVsUIHierarchyWindow> solutionExplorer)
            : base(SolutionNodeKind.Generic, hierarchyNode, nodeFactory, adapter, solutionExplorer)
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