﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Clide;

/// <summary>
/// Provides usability extensions to the <see cref="ISolutionNode"/> interface.
/// </summary>
[EditorBrowsable (EditorBrowsableState.Never)]
public static class ISolutionNodeExtensions
{
    /// <summary>
    /// Finds all the project nodes in the solution.
    /// </summary>
    /// <param name="solution">The solution to traverse.</param>
    /// <returns>All project nodes that were found.</returns>
    public static IEnumerable<IProjectNode> FindProjects(this ISolutionNode solution) =>
        solution.FindProjects(x => true);

	/// <summary>
	/// Finds all projects in the solution matching the given predicate.
	/// </summary>
	/// <param name="solution">The solution to traverse.</param>
	/// <param name="predicate">Predicate used to match projects.</param>
	/// <returns>All project nodes matching the given predicate that were found.</returns>
	public static IEnumerable<IProjectNode> FindProjects (this ISolutionNode solution, Func<IProjectNode, bool> predicate)
	{
		var visitor = new FilteringProjectsVisitor(predicate);

		solution.Accept (visitor);

		return visitor.Projects;
	}

	/// <summary>
	/// Finds the first project in the solution matching the given predicate.
	/// </summary>
	/// <param name="solution">The solution to traverse.</param>
	/// <param name="predicate">Predicate used to match projects.</param>
	/// <returns>The first project matching the given predicate, or <see langword="null"/>.</returns>
	public static IProjectNode FindProject (this ISolutionNode solution, Func<IProjectNode, bool> predicate)
	{
		var visitor = new FilteringProjectsVisitor(predicate, true);

		solution.Accept (visitor);

		return visitor.Projects.FirstOrDefault ();
	}

	class FilteringProjectsVisitor : ISolutionVisitor
	{
		Func<IProjectNode, bool> predicate;
		bool firstOnly;
		bool done;

		public FilteringProjectsVisitor (Func<IProjectNode, bool> predicate, bool firstOnly = false)
		{
			this.predicate = predicate;
			this.firstOnly = firstOnly;
			Projects = new List<IProjectNode> ();
		}

		public List<IProjectNode> Projects { get; private set; }

		/// <summary>
		/// Don't traverse project child elements.
		/// </summary>
		public bool VisitEnter (IProjectNode project)
		{
			if (!done && predicate (project)) {
				Projects.Add (project);
				if (firstOnly)
					done = true;
			}

			return false;
		}

		public bool VisitLeave (IProjectNode project) => !done;

		// Don't traverse child items of a solution item.
		public bool VisitEnter (ISolutionItemNode solutionItem) => false;

		public bool VisitLeave (ISolutionItemNode solutionItem) => true;

		public bool VisitEnter (ISolutionFolderNode solutionFolder) => !done;

		public bool VisitLeave (ISolutionFolderNode solutionFolder) => !done;

		public bool VisitEnter (ISolutionNode solution) => true;

		public bool VisitLeave (ISolutionNode solution) => true;

		public bool VisitEnter (IFolderNode folder) => false;

		public bool VisitLeave (IFolderNode folder) => false;

		public bool VisitEnter (IItemNode item) => false;

		public bool VisitLeave (IItemNode item) => false;

		public bool VisitEnter (IReferencesNode references) => false;

		public bool VisitLeave (IReferencesNode references) => false;

		public bool VisitEnter (IReferenceNode reference) => false;

		public bool VisitLeave (IReferenceNode reference) => false;

		public bool VisitEnter (IGenericNode node) => false;

		public bool VisitLeave (IGenericNode node) => true;
	}
}