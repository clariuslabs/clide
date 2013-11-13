#region BSD License
/* 
Copyright (c) 2010, NETFx
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion
#pragma warning disable 0436
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Traverses a tree (without checking for circular references) using 
/// a stack or queue based approach (no recursion), allowing infinitely 
/// deep trees.
/// </summary>
internal static class Traverser
{
	/// <summary>
	/// Traverses a tree using the given traversal <paramref name="kind"/>.
	/// </summary>
	/// <typeparam name="T">Type of the items to traverse, which can be inferred by the compiler so it's not necessary to specify it.</typeparam>
	/// <param name="source">The root items for the traversal, which are always included in the result of the traversal.</param>
	/// <param name="traverser">The traversing function that is applied to the current item of the type <typeparamref name="T"/>.</param>
	/// <returns>A flattened enumeration of the traversal, lazily evaluated.</returns>
	public static IEnumerable<T> Traverse<T>(this IEnumerable<T> source, TraverseKind kind, Func<T, IEnumerable<T>> traverser)
	{
		Guard.NotNull(() => source, source);
		Guard.NotNull(() => traverser, traverser);

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
#pragma warning restore 0436