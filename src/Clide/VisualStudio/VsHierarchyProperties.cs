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
    using Clide.Sdk.Solution;
    using Clide.Solution;
    using Clide.Solution.Implementation;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using System.Linq;

    internal class VsHierarchyProperties 
    {
        private IVsHierarchy hierarchy;

        public VsHierarchyProperties(IVsHierarchy hierarchy, uint itemId)
        {
            this.hierarchy = hierarchy;
            this.ItemId = itemId;
        }

        public uint ItemId { get; private set; }

        public string DisplayName
        {
            get { return GetProperty<string>(hierarchy, __VSHPROPID.VSHPROPID_Name, this.ItemId); }
        }

        public object ExtenderObject
        {
            get { return GetProperty<object>(hierarchy, __VSHPROPID.VSHPROPID_ExtObject, this.ItemId); }
        }

        public VsHierarchyItem Parent
        {
            get
            {
                var rootHierarchy = GetProperty<IVsHierarchy>(hierarchy, __VSHPROPID.VSHPROPID_ParentHierarchy, VSConstants.VSITEMID_ROOT);
                if (rootHierarchy == null)
                    return null;

                var rootNode = new VsSolutionHierarchyNode(rootHierarchy, VSConstants.VSITEMID_ROOT);
                var parentNode = new IVsSolutionHierarchyNode[] { rootNode }
                    .Concat(rootNode.Children.Traverse(TraverseKind.BreadthFirst, node => node.Children))
                    // This is expensive, but we only do it when the node was not created from traversal.
                    // TODO: Unfortunately, this is the case for SelectedNodes :(
                    .FirstOrDefault(node => node.Children.Any(child => child.ItemId == this.ItemId));

                // This should never happen.
                if (parentNode == null)
                    return null;

                return new VsHierarchyItem(
                    parentNode.VsHierarchy,
                    parentNode.ItemId);
            }
        }

        private static T GetProperty<T>(IVsHierarchy hierarchy, __VSHPROPID propId, uint itemid)
        {
            object value = null;
            int hr = hierarchy.GetProperty(itemid, (int)propId, out value);
            if (hr != VSConstants.S_OK || value == null)
            {
                return default(T);
            }
            return (T)value;
        }
    }
}
