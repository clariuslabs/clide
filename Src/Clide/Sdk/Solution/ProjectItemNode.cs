#region BSD License
/* 
Copyright (c) 2012, Clarius Consulting
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

namespace Clide.Sdk.Solution
{
	using Clide.Patterns.Adapter;
	using Clide.Solution;
	using Clide.Solution.Implementation;
	using Microsoft.VisualStudio;
	using System;

	/// <summary>
	/// Base class for nodes that exist with a managed project.
	/// </summary>
	public abstract class ProjectItemNode : SolutionTreeNode
	{
		private ITreeNodeFactory<IVsSolutionHierarchyNode> nodeFactory;
		private Lazy<IProjectNode> owningProject;

		/// <summary>
		/// Initializes a new instance of the <see cref="ProjectItemNode"/> class.
		/// </summary>
		/// <param name="kind">The kind of project node.</param>
		/// <param name="hierarchyNode">The underlying hierarchy represented by this node.</param>
		/// <param name="parentNode">The parent node accessor.</param>
		/// <param name="nodeFactory">The factory for child nodes.</param>
		/// <param name="adapter">The adapter service that implements the smart cast <see cref="ITreeNode.As{T}"/>.</param>
		public ProjectItemNode(
			SolutionNodeKind kind,
			IVsSolutionHierarchyNode hierarchyNode,
			Lazy<ITreeNode> parentNode,
			ITreeNodeFactory<IVsSolutionHierarchyNode> nodeFactory,
			IAdapterService adapter)
			: base(kind, hierarchyNode, parentNode, nodeFactory, adapter)
		{
			this.nodeFactory = nodeFactory;
			this.owningProject = new Lazy<IProjectNode>(() =>
			{
				var owningHierarchy = new VsSolutionHierarchyNode(hierarchyNode.VsHierarchy, VSConstants.VSITEMID_ROOT);
				return this.nodeFactory.CreateNode(GetParent(owningHierarchy), owningHierarchy) as IProjectNode;
			});
		}

		/// <summary>
		/// Gets the owning project.
		/// </summary>
		public virtual IProjectNode OwningProject
		{
			get { return this.owningProject.Value; }
		}

		private Lazy<ITreeNode> GetParent(IVsSolutionHierarchyNode hierarchy)
		{
			return hierarchy.Parent == null ? null :
			   new Lazy<ITreeNode>(() => this.nodeFactory.CreateNode(GetParent(hierarchy.Parent), hierarchy.Parent));
		}


		#region Equality

		/// <summary>
		/// Gets whether the given nodes are equal.
		/// </summary>
		public static bool operator ==(ProjectItemNode obj1, ProjectItemNode obj2)
		{
			return Equals(obj1, obj2);
		}

		/// <summary>
		/// Gets whether the given nodes are not equal.
		/// </summary>
		public static bool operator !=(ProjectItemNode obj1, ProjectItemNode obj2)
		{
			return !Equals(obj1, obj2);
		}

		/// <summary>
		/// Gets whether the current node equals the given node.
		/// </summary>
		public bool Equals(ProjectItemNode other)
		{
			return ProjectItemNode.Equals(this, other);
		}

		/// <summary>
		/// Gets whether the current node equals the given node.
		/// </summary>
		public override bool Equals(object obj)
		{
			return ProjectItemNode.Equals(this, obj as ProjectItemNode);
		}

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

			return obj1.OwningProject.Equals(obj2.OwningProject) &&
				obj1.HierarchyNode.ItemId.Equals(obj2.HierarchyNode.ItemId);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return OwningProject.GetHashCode() ^
				HierarchyNode.ItemId.GetHashCode();
		}

		#endregion
	}
}