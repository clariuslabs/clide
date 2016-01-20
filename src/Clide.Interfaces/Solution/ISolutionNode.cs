using System.Collections.Generic;

namespace Clide
{
	/// <summary>
	/// Represents the solution root node in the solution explorer tree.
	/// </summary>
	public interface ISolutionNode : ISolutionExplorerNode
	{
        /// <summary>
        /// Gets a value indicating whether a solution is open.
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// Gets the currently active project (if single), which can be the selected project, or 
        /// the project owning the currently selected item or opened designer file.
        /// </summary>
        /// <remarks>
        /// If there are multiple active projects, this property will be null. This can happen 
        /// when multiple selection is enabled for items across more than one project
        /// </remarks>
        IProjectNode ActiveProject { get; }

		/// <summary>
		/// Gets the physical path of the solution, if it has been saved already.
		/// </summary>
		string PhysicalPath { get; }

		/// <summary>
		/// Gets the currently selected nodes in the solution.
		/// </summary>
		IEnumerable<ISolutionExplorerNode> SelectedNodes { get; }

        /// <summary>
        /// Closes the solution.
        /// </summary>
        /// <param name="saveFirst">If set to <c>true</c> saves the solution before closing.</param>
        void Close(bool saveFirst = true);

        /// <summary>
        /// Creates a new blank solution with the specified solution file location.
        /// </summary>
        void Create(string solutionFile);

        /// <summary>
        /// Opens the specified solution file.
        /// </summary>
        void Open(string solutionFile);

        /// <summary>
        /// Saves the current solution.
        /// </summary>
        void Save();

        /// <summary>
        /// Saves the current solution to the specified target file.
        /// </summary>
        void SaveAs(string solutionFile);

        /// <summary>
        /// Creates a solution folder under the solution root.
        /// </summary>
		ISolutionFolderNode CreateSolutionFolder(string name);
    }
}
