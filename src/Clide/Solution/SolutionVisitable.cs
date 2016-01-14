namespace Clide
{
	/// <summary>
	/// Implements the hierarchical visitor traversal for the solution in a uniform way that 
	/// can be leveraged by custom nodes without having to reimplement the behavior.
	/// </summary>
	public static class SolutionVisitable
    {
        /// <summary>
        /// Visists the given visitor with the specified solution.
        /// </summary>
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

        /// <summary>
        /// Visists the given visitor with the specified solution folder.
        /// </summary>
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

        /// <summary>
        /// Visists the given visitor with the specified solution item.
        /// </summary>
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

        /// <summary>
        /// Visists the given visitor with the specified project.
        /// </summary>
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

        /// <summary>
        /// Visists the given visitor with the specified solution.
        /// </summary>
        public static bool Accept(IReferencesNode references, ISolutionVisitor visitor)
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

        /// <summary>
        /// Visists the given visitor with the specified reference.
        /// </summary>
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

        /// <summary>
        /// Visists the given visitor with the specified folder.
        /// </summary>
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

        /// <summary>
        /// Visists the given visitor with the specified item.
        /// </summary>
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

        /// <summary>
        /// Visists the given visitor with the specified custom node.
        /// </summary>
        public static bool Accept(IGenericNode node, ISolutionVisitor visitor)
        {
            if (visitor.VisitEnter(node))
            {
                foreach (var child in node.Nodes)
                {
                    if (!child.Accept(visitor))
                        break;
                }
            }

            return visitor.VisitLeave(node);
        }
    }
}