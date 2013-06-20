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

namespace Clide
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Provides node traversal extensions.
    /// </summary>
    public static class ITreeNodeExtensions
    {
        /// <summary>
        /// Traverses the specified node and all its descendents. The node itself 
        /// also exists in the returned enumeration. To traverse only the 
        /// descendents, traverse its <see cref="ITreeNode.Nodes"/> 
        /// property instead.
        /// </summary>
        /// <param name="node">The node to traverse.</param>
        /// <returns>The <paramref name="node"/> itself and all of its descendent nodes.</returns>
        public static IEnumerable<ITreeNode> Traverse(this ITreeNode node)
        {
            return new[] { node }.Traverse(TraverseKind.DepthFirst, x => x.Nodes);
        }

        /// <summary>
        /// Traverses the specified list of nodes and all their descendents. The nodes
        /// in the list are included in the returned enumeration.
        /// </summary>
        /// <param name="nodes">The nodes to traverse.</param>
        /// <returns>The <paramref name="nodes"/> themselves and all of their descendent nodes.</returns>
        public static IEnumerable<ITreeNode> Traverse(this IEnumerable<ITreeNode> nodes)
        {
            return nodes.Traverse(TraverseKind.DepthFirst, x => x.Nodes);
        }
    }
}
