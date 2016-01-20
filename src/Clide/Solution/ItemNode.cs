using System;
using System.IO;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide
{
	/// <summary>
	/// Default implementation of an item node in a managed project.
	/// </summary>
	public class ItemNode : ProjectItemNode, IItemNode
    {
        Lazy<ItemProperties> properties;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemNode"/> class.
        /// </summary>
        /// <param name="hierarchyNode">The underlying hierarchy represented by this node.</param>
        /// <param name="nodeFactory">The factory for child nodes.</param>
        /// <param name="adapter">The adapter service that implements the smart cast <see cref="ITreeNode.As{T}"/>.</param>
        public ItemNode(
            IVsHierarchyItem hierarchyNode,
			ISolutionExplorerNodeFactory nodeFactory,
            IAdapterService adapter, 
			Lazy<IVsUIHierarchyWindow> solutionExplorer)
            : base(SolutionNodeKind.Item, hierarchyNode, nodeFactory, adapter, solutionExplorer)
        {
            properties = new Lazy<ItemProperties>(() => new ItemProperties(this));
        }

		/// <summary>
		/// Gets the logical path of the item, relative to its containing project.
		/// </summary>
		public virtual string LogicalPath => this.RelativePathTo (OwningProject);

		/// <summary>
		/// Gets the physical path of the item.
		/// </summary>
		public virtual string PhysicalPath
		{
			get
			{
				var project = HierarchyNode.GetActualHierarchy() as IVsProject;
				string filePath;
				if (project != null && ErrorHandler.Succeeded (project.GetMkDocument (HierarchyNode.GetActualItemId (), out filePath)) &&
					File.Exists (filePath))
					return filePath;

				return HierarchyNode.CanonicalName;
			}
		}

		/// <summary>
		/// Gets the dynamic properties of the item.
		/// </summary>
		/// <remarks>
		/// The default implementation of item nodes exposes the
		/// MSBuild item metadata properties using this property,
		/// and allows getting and setting them.
		/// </remarks>
		public virtual dynamic Properties => properties.Value;

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