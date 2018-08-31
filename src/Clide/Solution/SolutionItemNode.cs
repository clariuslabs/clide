using System;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide
{
    /// <summary>
    /// Default implementation of a solution item node in a managed project.
    /// </summary>
    public class SolutionItemNode : SolutionExplorerNode, ISolutionItemNode
    {
        ISolutionExplorerNodeFactory nodeFactory;
        Lazy<ISolutionFolderNode> owningFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionItemNode"/> class.
        /// </summary>
        /// <param name="hierarchyNode">The underlying hierarchy represented by this node.</param>
        /// <param name="nodeFactory">The factory for child nodes.</param>
        /// <param name="adapter">The adapter service that implements the smart cast <see cref="ITreeNode.As{T}"/>.</param>
        public SolutionItemNode(
            IVsHierarchyItem hierarchyNode,
            ISolutionExplorerNodeFactory nodeFactory,
            IAdapterService adapter,
            JoinableLazy<IVsUIHierarchyWindow> solutionExplorer)
            : base(SolutionNodeKind.SolutionItem, hierarchyNode, nodeFactory, adapter, solutionExplorer)
        {
            this.nodeFactory = nodeFactory;

            Item = new Lazy<ProjectItem>(() => (ProjectItem)hierarchyNode.GetExtenderObject());
            owningFolder = new Lazy<ISolutionFolderNode>(() =>
                this.nodeFactory.CreateNode(hierarchyNode.GetRoot()) as ISolutionFolderNode);
        }

        /// <summary>
        /// Gets the logical path of the item, relative to the solution, 
        /// considering any containing solution folders.
        /// </summary>
        public virtual string LogicalPath => this.RelativePathTo(OwningSolution);

        /// <summary>
        /// Gets the physical path of the solution item.
        /// </summary>
        public virtual string PhysicalPath => Item.Value.get_FileNames(1);

        /// <summary>
        /// Gets the owning solution folder.
        /// </summary>
        public virtual ISolutionFolderNode OwningSolutionFolder => owningFolder.Value;

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

        /// <summary>
        /// Gets the item represented by this node.
        /// </summary>
        internal Lazy<ProjectItem> Item { get; }
    }
}