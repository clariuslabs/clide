using System.Collections.Generic;

namespace Clide
{    
    /// <summary>
    /// Exposes the solution explorer toolwindow.
    /// </summary>
	public interface ISolutionExplorer : IToolWindow
	{
        /// <summary>
        /// Gets the current solution, which might be an 
        /// empty one if no solution is open.
        /// </summary>
		ISolutionNode Solution { get; }

        /// <summary>
        /// Gets the currently selected nodes in the solution explorer tree, 
        /// which is retrieved from the <see cref="Solution"/> property 
        /// <see cref="ISolutionNode.SelectedNodes"/>.
        /// </summary>
        IEnumerable<ISolutionExplorerNode> SelectedNodes { get; }
	}
}
