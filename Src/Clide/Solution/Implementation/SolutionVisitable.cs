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
    using System.Linq;

    /// <summary>
    /// Implements the hierarchical visitor traversal for the solution.
    /// </summary>
    internal static class SolutionVisitable
    {
        public static bool Accept(ISolutionNode solution, ISolutionVisitor visitor)
        {
            if (visitor.VisitEnter(solution))
            {
                foreach (var node in solution.Nodes)
                {
                    if (!node.Accept(visitor))
                        break;
                }
            }

            return visitor.VisitLeave(solution);
        }

        public static bool Accept(ISolutionFolderNode solutionFolder, ISolutionVisitor visitor)
        {
            if (visitor.VisitEnter(solutionFolder))
            {
                foreach (var node in solutionFolder.Nodes)
                {
                    if (!node.Accept(visitor))
                        break;
                }
            }

            return visitor.VisitLeave(solutionFolder);
        }

        public static bool Accept(ISolutionItemNode solutionItem, ISolutionVisitor visitor)
        {
            if (visitor.VisitEnter(solutionItem))
            {
                foreach (var node in solutionItem.Nodes)
                {
                    if (!node.Accept(visitor))
                        break;
                }
            }

            return visitor.VisitLeave(solutionItem);
        }

        public static bool Accept(IProjectNode project, ISolutionVisitor visitor)
        {
            if (visitor.VisitEnter(project))
            {
                foreach (var node in project.Nodes)
                {
                    if (!node.Accept(visitor))
                        break;
                }
            }

            return visitor.VisitLeave(project);
        }

        public static bool Accept(ReferencesNode references, ISolutionVisitor visitor)
        {
            if (visitor.VisitEnter(references))
            {
                foreach (var node in references.Nodes)
                {
                    if (!node.Accept(visitor))
                        break;
                }
            }

            return visitor.VisitLeave(references);
        }

        public static bool Accept(IReferenceNode reference, ISolutionVisitor visitor)
        {
            if (visitor.VisitEnter(reference))
            {
                foreach (var node in reference.Nodes)
                {
                    if (!node.Accept(visitor))
                        break;
                }
            }

            return visitor.VisitLeave(reference);
        }

        public static bool Accept(IFolderNode folder, ISolutionVisitor visitor)
        {
            if (visitor.VisitEnter(folder))
            {
                foreach (var node in folder.Nodes)
                {
                    if (!node.Accept(visitor))
                        break;
                }
            }

            return visitor.VisitLeave(folder);
        }

        public static bool Accept(IItemNode item, ISolutionVisitor visitor)
        {
            if (visitor.VisitEnter(item))
            {
                foreach (var node in item.Nodes)
                {
                    if (!node.Accept(visitor))
                        break;
                }
            }

            return visitor.VisitLeave(item);
        }
    }
}