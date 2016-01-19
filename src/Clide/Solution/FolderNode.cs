using System;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide
{
    /// <summary>
    /// Default implementation of a folder node in a managed project.
    /// </summary>
    public class FolderNode : ProjectItemNode, IFolderNode
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="FolderNode"/> class.
        /// </summary>
        /// <param name="hierarchyNode">The underlying hierarchy represented by this node.</param>
        /// <param name="parentNode">The parent node accessor.</param>
        /// <param name="nodeFactory">The factory for child nodes.</param>
        /// <param name="adapter">The adapter service that implements the smart cast <see cref="ITreeNode.As{T}"/>.</param>
        public FolderNode(IVsHierarchyItem hierarchyNode,
			ISolutionExplorerNodeFactory nodeFactory,
			IAdapterService adapter, 
			Lazy<IVsUIHierarchyWindow> solutionExplorer)
            : base(SolutionNodeKind.Folder, hierarchyNode, nodeFactory, adapter, solutionExplorer)
		{
			Folder = new Lazy<ProjectItem>(() => hierarchyNode.GetExtenderObject() as ProjectItem);
		}

        /// <summary>
        /// Gets the folder project item corresponding to this node.
        /// </summary>
        internal Lazy<ProjectItem> Folder { get; private set; }

        /// <summary>
        /// Creates a nested folder.
        /// </summary>
        /// <param name="name">The name of the folder to create.</param>
        public virtual IFolderNode CreateFolder(string name)
		{
			Guard.NotNullOrEmpty(nameof (name), name);

			// NOTE: via DTE, you can't retrieve the created item/project/folder 
			// right from the method call, you need to find it afterwards.
			Folder.Value.ProjectItems.AddFolder(name);
			
			var newFolder = HierarchyNode.Children.Single(child => 
				child.GetProperty<string>(VsHierarchyPropID.Name) == name);

			return CreateNode(newFolder) as IFolderNode;
		}

		/// <summary>
		/// Accepts the specified visitor for traversal.
		/// </summary>
		public override bool Accept (ISolutionVisitor visitor) => SolutionVisitable.Accept (this, visitor);

		/// <summary>
		/// Tries to smart-cast this node to the give type.
		/// </summary>
		/// <typeparam name="T">Type to smart-cast to.</typeparam>
		/// <returns>
		/// The casted value or null if it cannot be converted to that type.
		/// </returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public override T As<T> () => Adapter.Adapt (this).As<T> ();
	}
}