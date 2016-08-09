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

namespace Clide.Solution
{
	using Clide.Diagnostics;
	using Clide.Properties;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Diagnostics;

	/// <summary>
	/// Provides usability extensions to the <see cref="ISolutionNode"/> interface.
	/// </summary>
	public static class ISolutionExplorerNodeExtensions
    {
        private static ITracer tracer = Tracer.Get(typeof(ISolutionExplorerNodeExtensions));

		/// <summary>
		/// Returns a relative (logical) path between a node and an ancestor.
		/// </summary>
		/// <param name="descendent">The descendent node to calculate the relative path for.</param>
		/// <param name="ancestor">The ancestor node that determines the root of the relative path.</param>
		/// <returns>The relative path from <paramref name="ancestor"/> to <paramref name="descendent"/>.</returns>
		/// <exception cref="System.ArgumentException">The <paramref name="ancestor"/> node is not actually 
		/// an ancestor of <paramref name="descendent"/>.</exception>
		public static string RelativePathTo(this ISolutionExplorerNode descendent, ISolutionExplorerNode ancestor)
		{
			if (!descendent.Ancestors().Any(node => node.Equals(ancestor)))
				throw new ArgumentException(Strings.ISolutionExplorerNodeExtensions.NotAncestor(ancestor, descendent));

			return string.Join(Path.DirectorySeparatorChar.ToString(), descendent
				.Ancestors()
				.TakeWhile(node => !node.Equals(ancestor))
				.Select(node => node.DisplayName)
				.Reverse()
				.Concat(new [] { descendent.DisplayName }));
		}
    }
}