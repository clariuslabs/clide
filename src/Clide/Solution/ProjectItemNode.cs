using System;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide
{
	/// <summary>
	/// Base class for nodes that exist with a managed project.
	/// </summary>
	public abstract class ProjectItemNode : SolutionExplorerNode
	{
		ISolutionExplorerNodeFactory nodeFactory;
		Lazy<IProjectNode> owningProject;

		/// <summary>
		/// Initializes a new instance of the <see cref="ProjectItemNode"/> class.
		/// </summary>
		/// <param name="kind">The kind of project node.</param>
		/// <param name="hierarchyNode">The underlying hierarchy represented by this node.</param>
		/// <param name="nodeFactory">The factory for child nodes.</param>
		/// <param name="adapter">The adapter service that implements the smart cast <see cref="ISolutionExplorerNode.As{T}"/>.</param>
		public ProjectItemNode(
			SolutionNodeKind kind,
			IVsHierarchyItem hierarchyNode,
			ISolutionExplorerNodeFactory nodeFactory,
			IAdapterService adapter,
			Lazy<IVsUIHierarchyWindow> solutionExplorer)
			: base(kind, hierarchyNode, nodeFactory, adapter, solutionExplorer)
		{
			this.nodeFactory = nodeFactory;
			owningProject = new Lazy<IProjectNode>(() => 
				this.nodeFactory.CreateNode(hierarchyNode.GetRoot ()) as IProjectNode);
		}

		/// <summary>
		/// Gets the owning project.
		/// </summary>
		public virtual IProjectNode OwningProject => owningProject.Value;

		Lazy<ISolutionExplorerNode> GetParent (IVsHierarchyItem hierarchy) => hierarchy.Parent == null ? null :
			new Lazy<ISolutionExplorerNode> (() => this.nodeFactory.CreateNode (hierarchy.Parent));

		#region Equality

		/// <summary>
		/// Gets whether the given nodes are equal.
		/// </summary>
		public static bool operator == (ProjectItemNode obj1, ProjectItemNode obj2) => Equals (obj1, obj2);

		/// <summary>
		/// Gets whether the given nodes are not equal.
		/// </summary>
		public static bool operator != (ProjectItemNode obj1, ProjectItemNode obj2) => !Equals (obj1, obj2);

		/// <summary>
		/// Gets whether the current node equals the given node.
		/// </summary>
		public bool Equals (ProjectItemNode other) => ProjectItemNode.Equals (this, other);

		/// <summary>
		/// Gets whether the current node equals the given node.
		/// </summary>
		public override bool Equals (object obj) => ProjectItemNode.Equals (this, obj as ProjectItemNode);

		/// <summary>
		/// Gets whether the given nodes are equal.
		/// </summary>
		public static bool Equals(ProjectItemNode obj1, ProjectItemNode obj2)
		{
			if (Object.Equals(null, obj1) ||
				Object.Equals(null, obj2) ||
				obj1.GetType() != obj2.GetType() ||
				Object.Equals(null, obj1.OwningProject) ||
				Object.Equals(null, obj2.OwningProject))
				return false;

			if (Object.ReferenceEquals(obj1, obj2)) return true;

			return obj1.HierarchyNode.GetActualHierarchy() == obj2.HierarchyNode.GetActualHierarchy() &&
				obj1.HierarchyNode.GetActualItemId() == obj2.HierarchyNode.GetActualItemId();
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode () =>
			HierarchyNode.GetActualHierarchy ().GetHashCode () ^
			HierarchyNode.GetActualItemId ().GetHashCode ();

		#endregion
	}
}