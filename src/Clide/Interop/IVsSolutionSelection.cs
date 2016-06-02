using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;

namespace Clide.Interop
{
	/// <summary>
	/// Provides a nicer abstraction over the VS solution hierarchy selection.
	/// </summary>
	public interface IVsSolutionSelection
	{
		/// <summary>
		/// If there is only one hierarchy in the current selection (whether there 
		/// are multiple selected items or not), returns that hierarchy, otherwise, 
		/// returns null.
		/// </summary>
		IVsHierarchyItem GetActiveHierarchy ();

		/// <summary>
		/// Gets all currently selected items.
		/// </summary>
		IEnumerable<IVsHierarchyItem> GetSelection ();
	}
}
