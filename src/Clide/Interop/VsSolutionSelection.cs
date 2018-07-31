using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;
using Merq;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide.Interop
{
    [Export(typeof(IVsSolutionSelection))]
    internal class VsSolutionSelection : IVsSolutionSelection
    {
        IVsUIHierarchyWindow hierarchyWindow;
        IVsHierarchyItemManager hierarchyManager;
        IAsyncManager asyncManager;

        [ImportingConstructor]
        public VsSolutionSelection(
            [Import(ContractNames.Interop.SolutionExplorerWindow)] IVsUIHierarchyWindow hierarchyWindow,
            IVsHierarchyItemManager hierarchyManager,
            IAsyncManager asyncManager)
        {
            this.hierarchyWindow = hierarchyWindow;
            this.hierarchyManager = hierarchyManager;
            this.asyncManager = asyncManager;
        }

        public IVsHierarchyItem GetActiveHierarchy()
        {
            return asyncManager.Run(async () =>
            {
                await asyncManager.SwitchToMainThread();
                IVsUIHierarchy uiHier;
                if (ErrorHandler.Failed(hierarchyWindow.FindCommonSelectedHierarchy((uint)__VSCOMHIEROPTIONS.COMHIEROPT_RootHierarchyOnly, out uiHier)))
                    return null;

                if (uiHier == null)
                    return null;

                return hierarchyManager.GetHierarchyItem(uiHier, VSConstants.VSITEMID_ROOT);
            });
        }

        public IEnumerable<IVsHierarchyItem> GetSelection()
        {
            return asyncManager.Run(async () =>
            {
                await asyncManager.SwitchToMainThread();

                var selHier = IntPtr.Zero;
                uint selId;
                IVsMultiItemSelect selMulti;

                try
                {
                    ErrorHandler.ThrowOnFailure(hierarchyWindow.GetCurrentSelection(out selHier, out selId, out selMulti));

                    // There may be no selection at all.
                    if (selMulti == null && selHier == IntPtr.Zero)
                        return Enumerable.Empty<IVsHierarchyItem>();

                    // This is a single item selection.
                    if (selMulti == null)
                    {
                        return new[] { hierarchyManager.GetHierarchyItem (
                            (IVsHierarchy)Marshal.GetTypedObjectForIUnknown (selHier, typeof (IVsHierarchy)), selId) };
                    }

                    // This is a multiple item selection.

                    uint selCount;
                    int singleHier;
                    ErrorHandler.ThrowOnFailure(selMulti.GetSelectionInfo(out selCount, out singleHier));

                    var selection = new VSITEMSELECTION[selCount];
                    ErrorHandler.ThrowOnFailure(selMulti.GetSelectedItems(0, selCount, selection));

                    return selection.Where(sel => sel.pHier != null)
                        .Select(sel => hierarchyManager.GetHierarchyItem(sel.pHier, sel.itemid))
                        .ToArray();

                }
                finally
                {
                    if (selHier != IntPtr.Zero)
                        Marshal.Release(selHier);
                }
            });
        }
    }
}
