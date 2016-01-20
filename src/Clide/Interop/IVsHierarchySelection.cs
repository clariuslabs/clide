using System.Collections.Generic;
using Microsoft.Internal.VisualStudio.PlatformUI;

namespace Clide.Interop
{
	/// <summary>
	/// Provides a nicer abstraction over a hierarchy selection.
	/// </summary>
	public interface IVsHierarchySelection
	{
		/// <summary>
		/// If there is only one hierarchy in the current selection (whether there 
		/// are multiple selected items or not), returns that hierarchy, otherwise, 
		/// returns null.
		/// </summary>
		HierarchyItemPair GetActiveHierarchy ();

		/// <summary>
		/// Gets all currently selected items.
		/// </summary>
		/// <returns></returns>
		IEnumerable<HierarchyItemPair> GetSelection ();
	}
}