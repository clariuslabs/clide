using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;

namespace Clide
{
    /// <summary>
    /// Low-level primitive that exposes the underlying 
    /// Visual Studio hierarchy nodes.
    /// </summary>
    public interface IVsSolutionItemNode
	{
		/// <summary>
		/// Gets the extensibility object.
		/// </summary>
		object ExtensibilityObject { get; }

		/// <summary>
		/// Gets the parent node of this item.
		/// </summary>
		IVsSolutionItemNode Parent { get; }

		/// <summary>
		/// Gets the service provider for the node.
		/// </summary>
		IServiceProvider ServiceProvider { get; }
        
        /// <summary>
        /// Gets the node hierarchy item. If the item 
		/// belongs to a nested hierarchy, this is the 
		/// actual parent nesting hierarchy, not the 
		/// nested one.
        /// </summary>
		IVsHierarchy VsHierarchy { get; }

        /// <summary>
        /// Gets the item id.
        /// </summary>
        uint ItemId { get; }

		/// <summary>
		/// Gets the underlying hierarchy item.
		/// </summary>
		IVsHierarchyItem HierarchyItem { get; }
	}
}
