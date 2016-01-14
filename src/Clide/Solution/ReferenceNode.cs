using System;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VSLangProj;

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
			Lazy<IVsUIHierarchyWindow> solutionExplorer)
            : base(SolutionNodeKind.Reference, hierarchyNode, nodeFactory, adapter, solutionExplorer)
		{
			Reference = new Lazy<Reference>(() => (Reference)hierarchyNode.GetExtenderObject());
		}

        /// <summary>
        /// Accepts the specified visitor for traversal.
        /// </summary>
        public override bool Accept(ISolutionVisitor visitor)
        {
            return SolutionVisitable.Accept(this, visitor);
        }

		/// <summary>
		/// Tries to smart-cast this node to the give type.
		/// </summary>
		/// <typeparam name="T">Type to smart-cast to.</typeparam>
		/// <returns>
		/// The casted value or null if it cannot be converted to that type.
		/// </returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public override T As<T>()
		{
			return Adapter.Adapt(this).As<T>();
		}

		/// <summary>
        /// Gets the reference represented by this node.
        /// </summary>
        internal Lazy<Reference> Reference { get; private set; }
	}
}