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
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;

    internal static class VsServiceProviderExtensions
    {
        public static VsToolWindow ToolWindow(this IServiceProvider serviceProvider, Guid toolWindowId)
        {
            return new VsToolWindow(serviceProvider, toolWindowId);
        }

        public static IEnumerable<Tuple<IVsHierarchy, uint>> GetSelection(this IServiceProvider serviceProvider)
        {
            var monitorSelection = serviceProvider.GetService<SVsShellMonitorSelection, IVsMonitorSelection>();
            var uiThread = serviceProvider.GetService<IUIThread>();

            var hierarchyPtr = IntPtr.Zero;
            var selectionContainer = IntPtr.Zero;
            

            return uiThread.Invoke<IEnumerable<Tuple<IVsHierarchy, uint>>>(() =>
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
                    {
                        if (hierarchyPtr == IntPtr.Zero)
                            return new[] { Tuple.Create(
                                (IVsHierarchy)serviceProvider.GetService<SVsSolution, IVsSolution>(), 
                                VSConstants.VSITEMID_ROOT) };
                        else
                            return new[] { Tuple.Create(
                                (IVsHierarchy)Marshal.GetTypedObjectForIUnknown(hierarchyPtr, typeof(IVsHierarchy)), 
                                VSConstants.VSITEMID_ROOT) };
                    }

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
    }
}
