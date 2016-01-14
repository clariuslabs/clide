using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Merq;

namespace Clide.Interop
{
	[Export (typeof (IVsHierarchySelection))]
	internal class VsHierarchySelection : IVsHierarchySelection
	{
		IVsHierarchy solution;
		IVsMonitorSelection monitorSelection;
		IAsyncManager asyncManager;

		[ImportingConstructor]
		public VsHierarchySelection (
			[Import (typeof (SVsServiceProvider))] IServiceProvider services,
			IAsyncManager asyncManager)
		{
			solution = services.GetService<SVsSolution, IVsHierarchy> ();
			monitorSelection = services.GetService<SVsShellMonitorSelection, IVsMonitorSelection> ();
			this.asyncManager = asyncManager;
		}

		public HierarchyItemPair GetActiveHierarchy ()
		{
			return asyncManager.Run (async () => {
				// The VS selection operations must be performed on the UI thread.
				await asyncManager.SwitchToMainThread ();

				var selHier = IntPtr.Zero;
				var selContainer = IntPtr.Zero;
				uint selId;
				IVsMultiItemSelect selMulti;

				try {
					// Get the current project hierarchy, project item, and selection container for the current selection
					// If the selection spans multiple hierarchies, hierarchyPtr is Zero
					ErrorHandler.ThrowOnFailure (monitorSelection.GetCurrentSelection (out selHier, out selId, out selMulti, out selContainer));

					// There may be no selection at all.
					if (selMulti == null && selHier == IntPtr.Zero)
						return null;

					// This is a single item selection, so we just grab the owning project/hierarchy.
					if (selMulti == null) {
						return new HierarchyItemPair (
							(IVsHierarchy)Marshal.GetTypedObjectForIUnknown (selHier, typeof (IVsHierarchy)), VSConstants.VSITEMID_ROOT);
					}

					// This is a multiple item selection.
					// If this is a multiple item selection within the same hierarchy,
					// we select the hierarchy too.
					uint selCount;
					int singleHier;
					ErrorHandler.ThrowOnFailure (selMulti.GetSelectionInfo (out selCount, out singleHier));

					if (singleHier == 1)
						return new HierarchyItemPair (
							(IVsHierarchy)Marshal.GetTypedObjectForIUnknown (selHier, typeof (IVsHierarchy)), VSConstants.VSITEMID_ROOT);

					return null;
				} finally {
					if (selHier != IntPtr.Zero) {
						Marshal.Release (selHier);
					}
					if (selContainer != IntPtr.Zero) {
						Marshal.Release (selContainer);
					}
				}
			});
		}

		public IEnumerable<HierarchyItemPair> GetSelection ()
		{
			return asyncManager.Run (async () => {
				// The VS selection operations must be performed on the UI thread.
				await asyncManager.SwitchToMainThread ();

				var selHier = IntPtr.Zero;
				var selContainer = IntPtr.Zero;
				uint selId;
				IVsMultiItemSelect selMulti;

				try {
					// Get the current project hierarchy, project item, and selection container for the current selection
					// If the selection spans multiple hierarchies, hierarchyPtr is Zero
					ErrorHandler.ThrowOnFailure (monitorSelection.GetCurrentSelection (out selHier, out selId, out selMulti, out selContainer));

					// There may be no selection at all.
					if (selMulti == null && selHier == IntPtr.Zero)
						return Enumerable.Empty<HierarchyItemPair> ();

					// This is a single item selection.
					if (selMulti == null) {
						return new[] { new HierarchyItemPair (
							(IVsHierarchy)Marshal.GetTypedObjectForIUnknown (selHier, typeof (IVsHierarchy)), selId) };
					}

					// This is a multiple item selection.
					uint selCount;
					int singleHier;
					ErrorHandler.ThrowOnFailure (selMulti.GetSelectionInfo (out selCount, out singleHier));

					var selection = new VSITEMSELECTION[selCount];
					ErrorHandler.ThrowOnFailure (selMulti.GetSelectedItems (0, selCount, selection));

					return selection.Where (sel => sel.pHier != null)
						.Select (sel => new HierarchyItemPair (sel.pHier, sel.itemid))
						.ToArray ();

				} finally {
					if (selHier != IntPtr.Zero) {
						Marshal.Release (selHier);
					}
					if (selContainer != IntPtr.Zero) {
						Marshal.Release (selContainer);
					}
				}
			});
		}
	}
}
