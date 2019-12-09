using System;
using System.Linq;
using Clide.Sdk;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide
{
    /// <summary>
    /// Default implementation of a solution folder node in a managed project.
    /// </summary>
    public class SolutionFolderNode : SolutionExplorerNode, ISolutionFolderNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionFolderNode"/> class.
        /// </summary>
        /// <param name="hierarchyNode">The underlying hierarchy represented by this node.</param>
        /// <param name="nodeFactory">The factory for child nodes.</param>
        /// <param name="adapter">The adapter service that implements the smart cast <see cref="ITreeNode.As{T}"/>.</param>
        public SolutionFolderNode(
            IVsHierarchyItem hierarchyNode,
            ISolutionExplorerNodeFactory nodeFactory,
            IAdapterService adapter,
            JoinableLazy<IVsUIHierarchyWindow> solutionExplorer)
            : base(SolutionNodeKind.SolutionFolder, hierarchyNode, nodeFactory, adapter, solutionExplorer)
        {
            SolutionFolder = new Lazy<SolutionFolder>(
                () => (SolutionFolder)((Project)hierarchyNode.GetExtenderObject()).Object);
        }

        /// <summary>
        /// Creates a nested solution folder.
        /// </summary>
        /// <param name="name">The name of the folder to create.</param>
        public virtual ISolutionFolderNode CreateSolutionFolder(string name)
        {
            Guard.NotNullOrEmpty(nameof(name), name);

            SolutionFolder.Value.AddSolutionFolder(name);

            var solutionfolder = HierarchyNode.Children.Single(child =>
                child.GetProperty<string>((int)VsHierarchyPropID.Name) == name);

            return CreateNode(solutionfolder) as ISolutionFolderNode;
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

        /// <summary>
        /// Gets the solution folder represented by this node.
        /// </summary>
        internal Lazy<SolutionFolder> SolutionFolder { get; }
    }
}