namespace Clide
{
    /// <summary>
    /// Provides a hierarchical visitor pattern interface for the solution model.
    /// </summary>
    public interface ISolutionVisitor
    {
        /// <summary>
        /// Begins visiting the solution.
        /// </summary>
        /// <param name="solution">The solution being visited.</param>
        /// <returns><see langword="true"/> if the solution child nodes should be visited; <see langword="false"/> otherwise.</returns>
        bool VisitEnter(ISolutionNode solution);

        /// <summary>
        /// Ends visiting the solution.
        /// </summary>
        /// <param name="solution">The solution being visited.</param>
        /// <returns>The result of the solution traversal operation.</returns>
        bool VisitLeave(ISolutionNode solution);

        /// <summary>
        /// Begins visiting a solution item.
        /// </summary>
        /// <param name="solutionItem">The solution item being visited.</param>
        /// <returns><see langword="true"/> if the solution item children should be visited; <see langword="false"/> otherwise.</returns>
        bool VisitEnter(ISolutionItemNode solutionItem);

        /// <summary>
        /// Ends visiting a solution item.
        /// </summary>
        /// <param name="solutionItem">The solution item being visited.</param>
        /// <returns><see langword="true"/> if the solution item siblings should be visited; <see langword="false"/> otherwise.</returns>
        bool VisitLeave(ISolutionItemNode solutionItem);

        /// <summary>
        /// Begins visiting a solution folder.
        /// </summary>
        /// <param name="solutionFolder">The solution folder being visited.</param>
        /// <returns><see langword="true"/> if the solution folder children should be visited; <see langword="false"/> otherwise.</returns>
        bool VisitEnter(ISolutionFolderNode solutionFolder);

        /// <summary>
        /// Ends visiting a solution folder.
        /// </summary>
        /// <param name="solutionFolder">The solution folder being visited.</param>
        /// <returns><see langword="true"/> if the solution folder siblings should be visited; <see langword="false"/> otherwise.</returns>
        bool VisitLeave(ISolutionFolderNode solutionFolder);

        /// <summary>
        /// Begins visiting a project.
        /// </summary>
        /// <param name="project">The project being visited.</param>
        /// <returns><see langword="true"/> if the project children should be visited; <see langword="false"/> otherwise.</returns>
        bool VisitEnter(IProjectNode project);

        /// <summary>
        /// Ends visiting a project.
        /// </summary>
        /// <param name="project">The project being visited.</param>
        /// <returns><see langword="true"/> if the project siblings should be visited; <see langword="false"/> otherwise.</returns>
        bool VisitLeave(IProjectNode project);

        /// <summary>
        /// Begins visiting a project folder.
        /// </summary>
        /// <param name="folder">The project folder being visited.</param>
        /// <returns><see langword="true"/> if the project folder children should be visited; <see langword="false"/> otherwise.</returns>
        bool VisitEnter(IFolderNode folder);

        /// <summary>
        /// Ends visiting a project folder.
        /// </summary>
        /// <param name="folder">The project folder being visited.</param>
        /// <returns><see langword="true"/> if the project folder siblings should be visited; <see langword="false"/> otherwise.</returns>
        bool VisitLeave(IFolderNode folder);

        /// <summary>
        /// Begins visiting a project item.
        /// </summary>
        /// <param name="item">The project item being visited.</param>
        /// <returns><see langword="true"/> if the project item children should be visited; <see langword="false"/> otherwise.</returns>
        bool VisitEnter(IItemNode item);

        /// <summary>
        /// Ends visiting a project item.
        /// </summary>
        /// <param name="item">The project item being visited.</param>
        /// <returns><see langword="true"/> if the project item siblings should be visited; <see langword="false"/> otherwise.</returns>
        bool VisitLeave(IItemNode item);

        /// <summary>
        /// Begins visiting a project references.
        /// </summary>
        /// <param name="references">The project's references being visited.</param>
        /// <returns><see langword="true"/> if the project references should be visited; <see langword="false"/> otherwise.</returns>
        bool VisitEnter(IReferencesNode references);

        /// <summary>
        /// Begins visiting a project references.
        /// </summary>
        /// <param name="references">The project's references being visited.</param>
        /// <returns><see langword="true"/> if the project references siblings should be visited; <see langword="false"/> otherwise.</returns>
        bool VisitLeave(IReferencesNode references);

        /// <summary>
        /// Begins visiting a project reference.
        /// </summary>
        /// <param name="reference">The project reference being visited.</param>
        /// <returns><see langword="true"/> if the project reference children should be visited; <see langword="false"/> otherwise.</returns>
        bool VisitEnter(IReferenceNode reference);

        /// <summary>
        /// Ends visiting a project reference.
        /// </summary>
        /// <param name="reference">The project reference being visited.</param>
        /// <returns><see langword="true"/> if the project reference siblings should be visited; <see langword="false"/> otherwise.</returns>
        bool VisitLeave(IReferenceNode reference);

		/// <summary>
		/// Begins visiting a generic or custom node.
		/// </summary>
		/// <param name="node">The generic or custom node being visited.</param>
		/// <returns><see langword="true"/> if the node children should be visited; <see langword="false"/> otherwise.</returns>
		bool VisitEnter (IGenericNode node);

		/// <summary>
		/// Ends visiting a generic or custom node.
		/// </summary>
		/// <param name="node">The generic or custom node being visited.</param>
		/// <returns><see langword="true"/> if the node siblings should be visited; <see langword="false"/> otherwise.</returns>
		bool VisitLeave (IGenericNode node);
    }
}