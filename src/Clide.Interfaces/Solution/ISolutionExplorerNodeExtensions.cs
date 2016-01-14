using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Clide;
using Clide.Properties.Interfaces;

/// <summary>
/// Provides usability extensions to the <see cref="ISolutionNode"/> interface.
/// </summary>
[EditorBrowsable (EditorBrowsableState.Never)]
public static class ISolutionExplorerNodeExtensions
{
	/// <summary>
	/// Traverses upwards the ancestors of the specified node.
	/// </summary>
	public static IEnumerable<ISolutionExplorerNode> Ancestors (this ISolutionExplorerNode node)
	{
		var parent = node.Parent;
		while (parent != null) {
			yield return parent;
			parent = parent.Parent;
		}
	}

	/// <summary>
	/// Returns a relative (logical) path between a node and an ancestor.
	/// </summary>
	/// <param name="descendent">The descendent node to calculate the relative path for.</param>
	/// <param name="ancestor">The ancestor node that determines the root of the relative path.</param>
	/// <returns>The relative path from <paramref name="ancestor"/> to <paramref name="descendent"/>.</returns>
	/// <exception cref="System.ArgumentException">The <paramref name="ancestor"/> node is not actually 
	/// an ancestor of <paramref name="descendent"/>.</exception>
	public static string RelativePathTo (this ISolutionExplorerNode descendent, ISolutionExplorerNode ancestor)
	{
		if (!descendent.Ancestors ().Any (node => node.Equals (ancestor) || ReferenceEquals(node, ancestor)))
			throw new ArgumentException (Strings.ISolutionExplorerNodeExtensions.NotAncestor (ancestor, descendent));

		return string.Join (Path.DirectorySeparatorChar.ToString (), descendent
			.Ancestors ()
			.TakeWhile (node => !node.Equals (ancestor) && !ReferenceEquals(node, ancestor))
			.Select (node => node.Name)
			.Reverse ()
			.Concat (new[] { descendent.Name }));
	}
}