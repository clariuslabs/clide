using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Traverses a tree (without checking for circular references) using 
/// a stack or queue based approach (no recursion), allowing infinitely 
/// deep trees.
/// </summary>
/// <nuget id="netfx-System.Collections.Generic.IEnumerable.Traverse" />
internal static class Traverser
{
	/// <summary>
	/// Traverses a tree using the given traversal <paramref name="kind"/>.
	/// </summary>
	/// <nuget id="netfx-System.Collections.Generic.IEnumerable.Traverse" />
	/// <typeparam name="T">Type of the items to traverse, which can be inferred by the compiler so it's not necessary to specify it.</typeparam>
	/// <param name="source" this="true">The root items for the traversal, which are always included in the result of the traversal.</param>
	/// <param name="kind">Traversal style to use. See <see cref="TraverseKind"/>.</param>
	/// <param name="traverser">The traversing function that is applied to the current item of the type <typeparamref name="T"/>.</param>
	/// <returns>A flattened enumeration of the traversal, lazily evaluated.</returns>
	public static IEnumerable<T> Traverse<T>(this IEnumerable<T> source, TraverseKind kind, Func<T, IEnumerable<T>> traverser)
	{
		if (kind == TraverseKind.BreadthFirst)
			return source.TraverseBreadthFirst(traverser);
		else
			return source.TraverseDepthFirst(traverser);
	}

	private static IEnumerable<T> TraverseBreadthFirst<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> traverser)
	{
		var queue = new Queue<T>();

		foreach (var item in source)
		{
			queue.Enqueue(item);
		}

		while (queue.Count > 0)
		{
			var current = queue.Dequeue();
			yield return current;

			var children = traverser(current);
			if (children != null)
			{
				foreach (var child in traverser(current))
				{
					queue.Enqueue(child);
				}
			}
		}
	}

	private static IEnumerable<T> TraverseDepthFirst<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> traverser)
	{
		var stack = new Stack<T>();

		foreach (var item in source)
		{
			stack.Push(item);
		}

		while (stack.Count > 0)
		{
			var current = stack.Pop();
			yield return current;

			var children = traverser(current);
			if (children != null)
			{
				foreach (var child in traverser(current))
				{
					stack.Push(child);
				}
			}
		}
	}
}