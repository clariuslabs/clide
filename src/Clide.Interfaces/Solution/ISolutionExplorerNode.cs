using System;
using System.Collections.Generic;

namespace Clide
{
	/// <summary>
	/// Interface implemented by all nodes in the solution explorer tree.
	/// </summary>
	public interface ISolutionExplorerNode : IFluentInterface, IEquatable<ISolutionExplorerNode>
	{
		/// <summary>
		/// Gets the node name.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets the node text as shown in the solution explorer window. 
		/// May not be the same as the <see cref="Name"/> (i.e. for the 
		/// solution node itself, it isn't).
		/// </summary>
		string Text { get; }

		/// <summary>
		/// Gets the kind of node.
		/// </summary>
		SolutionNodeKind Kind { get; }

		/// <summary>
		/// Gets a value indicating whether this node is hidden.
		/// </summary>
		bool IsHidden { get; }

		/// <summary>
		/// Gets a value indicating whether this node is visible by the user.
		/// </summary>
		bool IsVisible { get; }

		/// <summary>
		/// Gets a value indicating whether this node is selected.
		/// </summary>
		bool IsSelected { get; }

		/// <summary>
		/// Gets a value indicating whether this node is expanded.
		/// </summary>
		bool IsExpanded { get; }

		/// <summary>
		/// Gets the child nodes.
		/// </summary>
		IEnumerable<ISolutionExplorerNode> Nodes { get; }

		/// <summary>
		/// Gets the owning solution.
		/// </summary>
		ISolutionNode OwningSolution { get; }

		/// <summary>
		/// Gets the parent of this node. May be null, such as for the root solution node.
		/// </summary>
		ISolutionExplorerNode Parent { get; }

		/// <summary>
		/// Tries to smart-cast this node to the give type.
		/// </summary>
		/// <typeparam name="T">Type to smart-cast to.</typeparam>
		/// <returns>The casted value or null if it cannot be converted to that type.</returns>
		T As<T>() where T : class;

		/// <summary>
		/// Collapses this node.
		/// </summary>
		void Collapse ();

		/// <summary>
		/// Expands the node, optionally in a recursive fashion.
		/// </summary>
		/// <param name="recursively">if set to <c>true</c>, expands recursively</param>
		void Expand (bool recursively = false);

		/// <summary>
		/// Selects the node, optionally allowing multiple selection.
		/// </summary>
		/// <param name="allowMultiple">if set to <c>true</c>, adds this node to the current selection.</param>
		void Select (bool allowMultiple = false);

		/// <summary>
		/// Accepts the specified visitor for traversal.
		/// </summary>
		/// <returns><see langword="true"/> if the operation should continue with other sibling or child nodes; <see langword="false"/> otherwise.</returns>
		bool Accept (ISolutionVisitor visitor);
    }
}
