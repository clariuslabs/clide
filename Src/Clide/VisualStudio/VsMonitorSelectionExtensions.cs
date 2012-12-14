#region BSD License
/* 
Copyright (c) 2012, Clarius Consulting
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

namespace Clide.VisualStudio
{
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell;

    internal static class VsMonitorSelectionExtensions
    {
        public static IEnumerable<Tuple<IVsHierarchy, uint>> GetSelection(this IVsMonitorSelection monitorSelection)
        {
            //return GetCurrentSelection(monitorSelection).Select(item => Tuple.Create(item.pHier, item.itemid));
            var hierarchyPtr = IntPtr.Zero;
            var selectionContainer = IntPtr.Zero;

            return ThreadHelper.Generic.Invoke<IEnumerable<Tuple<IVsHierarchy, uint>>>(() =>
            {
                try
                {
                    // Get the current project hierarchy, project item, and selection container for the current selection
                    // If the selection spans multiple hierarchies, hierarchyPtr is Zero
                    uint itemid;
                    IVsMultiItemSelect multiItemSelect = null;
                    ErrorHandler.ThrowOnFailure(monitorSelection.GetCurrentSelection(out hierarchyPtr, out itemid, out multiItemSelect, out selectionContainer));

                    if (itemid == VSConstants.VSITEMID_NIL)
                        return Enumerable.Empty<Tuple<IVsHierarchy, uint>>();

                    if (itemid == VSConstants.VSITEMID_ROOT)
                        return new[] { Tuple.Create(
                        (IVsHierarchy)Marshal.GetTypedObjectForIUnknown(hierarchyPtr, typeof(IVsHierarchy)), 
                        VSConstants.VSITEMID_ROOT) };

                    if (itemid != VSConstants.VSITEMID_SELECTION)
                        return new[] { Tuple.Create(
                        (IVsHierarchy)Marshal.GetTypedObjectForIUnknown(hierarchyPtr, typeof(IVsHierarchy)), 
                        itemid) };

                    // This is a multiple item selection.

                    uint numberOfSelectedItems;
                    int isSingleHierarchyInt;
                    ErrorHandler.ThrowOnFailure(multiItemSelect.GetSelectionInfo(out numberOfSelectedItems, out isSingleHierarchyInt));
                    var isSingleHierarchy = (isSingleHierarchyInt != 0);

                    var vsItemSelections = new VSITEMSELECTION[numberOfSelectedItems];
                    var flags = (isSingleHierarchy) ? (uint)__VSGSIFLAGS.GSI_fOmitHierPtrs : 0;
                    ErrorHandler.ThrowOnFailure(multiItemSelect.GetSelectedItems(flags, numberOfSelectedItems, vsItemSelections));

                    return vsItemSelections.Where(sel => sel.pHier != null)
                        .Select(sel => Tuple.Create(sel.pHier, sel.itemid));
                }
                finally
                {
                    if (hierarchyPtr != IntPtr.Zero)
                    {
                        Marshal.Release(hierarchyPtr);
                    }
                    if (selectionContainer != IntPtr.Zero)
                    {
                        Marshal.Release(selectionContainer);
                    }
                }
            });
        }

        private static IEnumerable<VSITEMSELECTION> GetCurrentSelection(this IVsMonitorSelection window)
        {
            IntPtr pUnk;
            uint itemId;
            IVsMultiItemSelect mis;
            IntPtr ppSC = IntPtr.Zero;

            try
            {
                if (ErrorHandler.Succeeded(window.GetCurrentSelection(out pUnk, out itemId, out mis, out ppSC)))
                {
                    uint count;
                    int singleHierarchy;

                    if (mis != null && ErrorHandler.Succeeded(mis.GetSelectionInfo(out count, out singleHierarchy)))
                    {
                        __VSGSIFLAGS options = 0;
                        VSITEMSELECTION[] selection = new VSITEMSELECTION[count];

                        if (ErrorHandler.Succeeded(mis.GetSelectedItems((uint)options, count, selection)))
                        {
                            foreach (VSITEMSELECTION item in selection)
                                yield return item;
                        }
                    }
                    else
                    {
                        IVsHierarchy hierarchy = Marshal.GetObjectForIUnknown(pUnk) as IVsHierarchy;
                        if (hierarchy != null)
                        {
                            yield return new VSITEMSELECTION()
                            {
                                pHier = hierarchy,
                                itemid = itemId
                            };
                        }
                    }
                }

            }
            finally
            {
                if (ppSC != IntPtr.Zero)
                    Marshal.Release(ppSC);
            }
        }
    }
}
