using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Ole = Microsoft.VisualStudio.OLE.Interop;

namespace Clide
{
	/// <summary>
	/// Provides extensions for Visual Studio low-level <see cref="IVsHierarchyItem"/>.
	/// </summary>
	internal static class IVsHierarchyItemExtensions
	{
		/// <summary>
		/// Gets the actual item hierarchy, which is the nested hierarchy if the item is nested.
		/// </summary>
		public static IVsHierarchy GetActualHierarchy(this IVsHierarchyItem item)
		{
			return item.HierarchyIdentity.IsNestedItem ?
				item.HierarchyIdentity.NestedHierarchy :
				item.HierarchyIdentity.Hierarchy;
		}

		/// <summary>
		/// Gets the actual item id, which is the nested id if the item is nested.
		/// </summary>
		public static uint GetActualItemId (this IVsHierarchyItem item)
		{
			return item.HierarchyIdentity.IsNestedItem ?
				item.HierarchyIdentity.NestedItemID :
				item.HierarchyIdentity.ItemID;
		}

		/// <summary>
		/// Gets the DTE extensibility object for the given hierarchy item.
		/// </summary>
		public static object GetExtenderObject(this IVsHierarchyItem item)
		{
			return item.GetProperty((int)__VSHPROPID.VSHPROPID_ExtObject);
		}

		/// <summary>
		/// Gets the DTE extensibility object for the given hierarchy item.
		/// </summary>
		public static T GetExtenderObject<T>(this IVsHierarchyItem item)
		{
			return (T)GetExtenderObject(item);
		}

		public static object GetProperty (this IVsHierarchyItem item, int propId, object defaultValue = null)
		{
			return GetProperty<object> (item, propId, defaultValue);
		}

		public static T GetProperty<T>(this IVsHierarchyItem item, __VSPROPID propId, T defaultValue = default (T))
		{
			return item.GetProperty<T> ((int)propId, defaultValue);
		}

		public static T GetProperty<T>(this IVsHierarchyItem item, __VSPROPID2 propId, T defaultValue = default (T))
		{
			return item.GetProperty<T> ((int)propId, defaultValue);
		}

		public static T GetProperty<T>(this IVsHierarchyItem item, __VSPROPID3 propId, T defaultValue = default (T))
		{
			return item.GetProperty<T> ((int)propId, defaultValue);
		}

		public static T GetProperty<T>(this IVsHierarchyItem item, __VSPROPID4 propId, T defaultValue = default (T))
		{
			return item.GetProperty<T> ((int)propId, defaultValue);
		}

		public static T GetProperty<T>(this IVsHierarchyItem item, __VSPROPID5 propId, T defaultValue = default (T))
		{
			return item.GetProperty<T> ((int)propId, defaultValue);
		}

		public static T GetProperty<T>(this IVsHierarchyItem item, VsHierarchyPropID propId, T defaultValue = default (T))
		{
			return item.GetProperty<T> ((int)propId, defaultValue);
		}

		public static T GetProperty<T>(this IVsHierarchyItem item, int propId, T defaultValue = default (T))
		{
			object value = null;

			// If item is nested, first try to get the property from the nested hierarchy, 
			// and default to the parent otherwise.
			if (item.HierarchyIdentity.IsNestedItem) {
				if (ErrorHandler.Succeeded (item.HierarchyIdentity.NestedHierarchy.GetProperty (item.HierarchyIdentity.NestedItemID, propId, out value)) &&
					value != null) {
					return (T)value;
				}

				// If the item is nested but is a root, then if the property does not exit, 
				// we just exit, since we don't want to accidentally retrieve the propertly 
				// for the nesting hierarchy.
				if (item.HierarchyIdentity.IsRoot)
					return defaultValue;
			}

			if (ErrorHandler.Succeeded (item.HierarchyIdentity.Hierarchy.GetProperty (item.HierarchyIdentity.ItemID, propId, out value)) &&
				value != null) {
				return (T)value;
			}

			return defaultValue;
		}

		/// <summary>
		/// Gets the service provider for this item.
		/// </summary>
		public static IServiceProvider GetServiceProvider (this IVsHierarchyItem item)
		{
			Ole.IServiceProvider oleSp;
			if (item.HierarchyIdentity.IsNestedItem &&
				ErrorHandler.Succeeded (item.HierarchyIdentity.NestedHierarchy.GetSite (out oleSp)) &&
				oleSp != null) {
				return new OleServiceProvider (oleSp);
			}

			if (ErrorHandler.Succeeded (item.HierarchyIdentity.Hierarchy.GetSite (out oleSp)) &&
				oleSp != null) {
				return new OleServiceProvider (oleSp);
			}

			// Try the hierarchy root as a fallback.
			var root = item.GetRoot();
			if (root != null &&
				ErrorHandler.Succeeded (root.HierarchyIdentity.Hierarchy.GetSite (out oleSp)) &&
				oleSp != null) {
				return new OleServiceProvider (oleSp);
			}

			// Try the hierarchy top-most node as a fallback (this would be the solution itself)
			root = item.GetTopMost ();
			if (root != null &&
				ErrorHandler.Succeeded (root.HierarchyIdentity.Hierarchy.GetSite (out oleSp)) &&
				oleSp != null) {
				return new OleServiceProvider (oleSp);
			}

			// Default to a global service provider provided by VS shell.
			// TODO: may require us to switch to the UI thread to get the right one.
			return ServiceProvider.Global;
		}

		/// <summary>
		/// Walks up the <see cref="IVsHierarchyItem.Parent"/> until 
		/// it reaches the top-most item, the one without a parent.
		/// </summary>
		public static IVsHierarchyItem GetTopMost (this IVsHierarchyItem item)
		{
			var current = item;
			while (current.Parent != null) {
				current = current.Parent;
			}

			return current;
		}

		/// <summary>
		/// Walks up the <see cref="IVsHierarchyItem.Parent"/> while the 
		/// <see cref="IVsHierarchyItemIdentity.IsRoot"/> is false or we 
		/// reach a null parent.
		/// </summary>
		public static IVsHierarchyItem GetRoot (this IVsHierarchyItem item)
		{
			var current = item;
			while (current != null && !current.HierarchyIdentity.IsRoot) {
				current = current.Parent;
			}

			return current;
		}
	}
}
