namespace Clide
{

    /// <summary>
    /// Convenience default implementation of <see cref="ISolutionVisitor"/> that 
    /// just visits the entire solution hierarchy.
    /// </summary>
    public abstract class SolutionVisitor : ISolutionVisitor
    {
		/// <summary>
		/// Begins visiting the solution.
		/// </summary>
		/// <param name="solution">The solution being visited.</param>
		/// <returns>
		///   <see langword="true" /> if the solution child nodes should be visited; <see langword="false" /> otherwise.
		/// </returns>
		public virtual bool VisitEnter (ISolutionNode solution) => true;

		/// <summary>
		/// Ends visiting the solution.
		/// </summary>
		/// <param name="solution">The solution being visited.</param>
		/// <returns>
		/// The result of the solution traversal operation.
		/// </returns>
		public virtual bool VisitLeave (ISolutionNode solution) => true;

		/// <summary>
		/// Begins visiting a solution item.
		/// </summary>
		/// <param name="solutionItem">The solution item being visited.</param>
		/// <returns>
		///   <see langword="true" /> if the solution item children should be visited; <see langword="false" /> otherwise.
		/// </returns>
		public virtual bool VisitEnter (ISolutionItemNode solutionItem) => true;

		/// <summary>
		/// Ends visiting a solution item.
		/// </summary>
		/// <param name="solutionItem">The solution item being visited.</param>
		/// <returns>
		///   <see langword="true" /> if the solution item siblings should be visited; <see langword="false" /> otherwise.
		/// </returns>
		public virtual bool VisitLeave (ISolutionItemNode solutionItem) => true;

		/// <summary>
		/// Begins visiting a solution folder.
		/// </summary>
		/// <param name="solutionFolder">The solution folder being visited.</param>
		/// <returns>
		///   <see langword="true" /> if the solution folder children should be visited; <see langword="false" /> otherwise.
		/// </returns>
		public virtual bool VisitEnter (ISolutionFolderNode solutionFolder) => true;

		/// <summary>
		/// Ends visiting a solution folder.
		/// </summary>
		/// <param name="solutionFolder">The solution folder being visited.</param>
		/// <returns>
		///   <see langword="true" /> if the solution folder siblings should be visited; <see langword="false" /> otherwise.
		/// </returns>
		public virtual bool VisitLeave (ISolutionFolderNode solutionFolder) => true;

		/// <summary>
		/// Begins visiting a project.
		/// </summary>
		/// <param name="project">The project being visited.</param>
		/// <returns>
		///   <see langword="true" /> if the project children should be visited; <see langword="false" /> otherwise.
		/// </returns>
		public virtual bool VisitEnter (IProjectNode project) => true;

		/// <summary>
		/// Ends visiting a project.
		/// </summary>
		/// <param name="project">The project being visited.</param>
		/// <returns>
		///   <see langword="true" /> if the project siblings should be visited; <see langword="false" /> otherwise.
		/// </returns>
		public virtual bool VisitLeave (IProjectNode project) => true;

		/// <summary>
		/// Begins visiting a project folder.
		/// </summary>
		/// <param name="folder">The project folder being visited.</param>
		/// <returns>
		///   <see langword="true" /> if the project folder children should be visited; <see langword="false" /> otherwise.
		/// </returns>
		public virtual bool VisitEnter (IFolderNode folder) => true;

		/// <summary>
		/// Ends visiting a project folder.
		/// </summary>
		/// <param name="folder">The project folder being visited.</param>
		/// <returns>
		///   <see langword="true" /> if the project folder siblings should be visited; <see langword="false" /> otherwise.
		/// </returns>
		public virtual bool VisitLeave (IFolderNode folder) => true;

		/// <summary>
		/// Begins visiting a project item.
		/// </summary>
		/// <param name="item">The project item being visited.</param>
		/// <returns>
		///   <see langword="true" /> if the project item children should be visited; <see langword="false" /> otherwise.
		/// </returns>
		public virtual bool VisitEnter (IItemNode item) => true;

		/// <summary>
		/// Ends visiting a project item.
		/// </summary>
		/// <param name="item">The project item being visited.</param>
		/// <returns>
		///   <see langword="true" /> if the project item siblings should be visited; <see langword="false" /> otherwise.
		/// </returns>
		public virtual bool VisitLeave (IItemNode item) => true;

		/// <summary>
		/// Begins visiting a project references.
		/// </summary>
		/// <param name="references">The project's references being visited.</param>
		/// <returns>
		///   <see langword="true" /> if the project references should be visited; <see langword="false" /> otherwise.
		/// </returns>
		public virtual bool VisitEnter (IReferencesNode references) => true;

		/// <summary>
		/// Begins visiting a project references.
		/// </summary>
		/// <param name="references">The project's references being visited.</param>
		/// <returns>
		///   <see langword="true" /> if the project references siblings should be visited; <see langword="false" /> otherwise.
		/// </returns>
		public virtual bool VisitLeave (IReferencesNode references) => true;

		/// <summary>
		/// Begins visiting a project reference.
		/// </summary>
		/// <param name="reference">The project reference being visited.</param>
		/// <returns>
		///   <see langword="true" /> if the project reference children should be visited; <see langword="false" /> otherwise.
		/// </returns>
		public virtual bool VisitEnter (IReferenceNode reference) => true;

		/// <summary>
		/// Ends visiting a project reference.
		/// </summary>
		/// <param name="reference">The project reference being visited.</param>
		/// <returns>
		///   <see langword="true" /> if the project reference siblings should be visited; <see langword="false" /> otherwise.
		/// </returns>
		public virtual bool VisitLeave (IReferenceNode reference) => true;

		/// <summary>
		/// Begins visiting a custom node.
		/// </summary>
		/// <param name="customNode">The custom node being visited.</param>
		/// <returns><see langword="true"/> if the node children should be visited; <see langword="false"/> otherwise.</returns>
		public bool VisitEnterCustom (ISolutionExplorerNode customNode) => true;

		/// <summary>
		/// Ends visiting a custom node.
		/// </summary>
		/// <param name="customNode">The custom node being visited.</param>
		/// <returns><see langword="true"/> if the node siblings should be visited; <see langword="false"/> otherwise.</returns>
		public bool VisitLeaveCustom (ISolutionExplorerNode customNode) => true;


		/// <summary>
		/// Begins visiting a custom node.
		/// </summary>
		/// <param name="node">The custom node being visited.</param>
		/// <returns><see langword="true" /> if the node children should be visited; <see langword="false" /> otherwise.</returns>
		public bool VisitEnter (IGenericNode node) => true;

		/// <summary>
		/// Ends visiting a custom node.
		/// </summary>
		/// <param name="node">The custom node being visited.</param>
		/// <returns><see langword="true" /> if the node siblings should be visited; <see langword="false" /> otherwise.</returns>
		public bool VisitLeave (IGenericNode node) => true;
	}
}