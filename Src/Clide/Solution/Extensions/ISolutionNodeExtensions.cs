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
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Provides usability extensions to the <see cref="ISolutionNode"/> interface.
    /// </summary>
    public static class ISolutionNodeExtensions
    {
        /// <summary>
        /// Finds all the project nodes in the solution.
        /// </summary>
        /// <param name="solution">The solution to traverse.</param>
        /// <returns>All project nodes that were found.</returns>
        public static IEnumerable<IProjectNode> FindProjects(this ISolutionNode solution)
        {
            return solution.Nodes.OfType<ISolutionExplorerNode>()
                .Traverse(TraverseKind.DepthFirst, node =>
                    // Note: we only keep traversing if the node is a solution folder.
                    // This will significantly improve performance since it will not 
                    // traverse project items unnecessarily, since projects cannot 
                    // in turn contain other projects, just like items and other nodes.
                    node.Kind == SolutionNodeKind.SolutionFolder ?
                        node.Nodes.OfType<ISolutionExplorerNode>() :
                        Enumerable.Empty<ISolutionExplorerNode>())
                .OfType<IProjectNode>();
        }
    }
}