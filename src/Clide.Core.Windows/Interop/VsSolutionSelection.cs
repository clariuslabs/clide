using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;
using Merq;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;

namespace Clide.Interop
{
    [Export(typeof(IVsSolutionSelection))]
    internal class VsSolutionSelection : IVsSolutionSelection
    {
        IVsUIHierarchyWindow hierarchyWindow;
        IVsHierarchyItemManager hierarchyManager;
        JoinableTaskFactory jtf;

        [ImportingConstructor]
        public VsSolutionSelection(
            [Import(ContractNames.Interop.SolutionExplorerWindow)] IVsUIHierarchyWindow hierarchyWindow,
            IVsHierarchyItemManager hierarchyManager,
            JoinableTaskContext context)
        {
            this.hierarchyWindow = hierarchyWindow;
            this.hierarchyManager = hierarchyManager;
            jtf = context.Factory;
        }

        public IVsHierarchyItem GetActiveHierarchy()
        {
            return jtf.Run(async () =>
            {
                await jtf.SwitchToMainThreadAsync();
                if (ErrorHandler.Failed(hierarchyWindow.FindCommonSelectedHierarchy((uint)__VSCOMHIEROPTIONS.COMHIEROPT_RootHierarchyOnly, out var uiHier)) ||
                    uiHier == null)
                {
                    return null;
                }

                return hierarchyManager.GetHierarchyItem(uiHier, VSConstants.VSITEMID_ROOT);
            });
        }

        public IEnumerable<IVsHierarchyItem> GetSelection()
        {
            return jtf.Run(async () =>
            {
                await jtf.SwitchToMainThreadAsync();

                var selHier = IntPtr.Zero;

                try
                {
                    ErrorHandler.ThrowOnFailure(hierarchyWindow.GetCurrentSelection(out selHier, out var selId, out var selMulti));

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

                    ErrorHandler.ThrowOnFailure(selMulti.GetSelectionInfo(out var selCount, out var singleHier));

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
