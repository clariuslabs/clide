#region BSD License
/* 
Copyright (c) 2012, Clarius Consulting
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

namespace Clide.Solution.Implementation
{
    using Clide.Sdk.Solution;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using System.Collections;
    using System.Collections.Generic;

    internal class VsSolutionHierarchyNodeIterator : IEnumerable<IVsSolutionHierarchyNode>
    {
        private VsSolutionHierarchyNode parent;

        public VsSolutionHierarchyNodeIterator(VsSolutionHierarchyNode parent)
        {
            this.parent = parent;
        }

        public bool IsSolution
        {
            get { return this.parent.VsHierarchy is IVsSolution; }
        }

        public IEnumerator<IVsSolutionHierarchyNode> GetEnumerator()
        {
            foreach (var node in Enumerate(this.parent, this.IsSolution))
            {
                yield return node;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Performs the actual enumeration and factory invocation.
        /// </summary>
        private static IEnumerable<IVsSolutionHierarchyNode> Enumerate(VsSolutionHierarchyNode parent, bool isSolution)
        {
            int hr;
            object pVar;

            hr = parent.VsHierarchy.GetProperty(parent.ItemId, (int)(isSolution ? __VSHPROPID.VSHPROPID_FirstVisibleChild : __VSHPROPID.VSHPROPID_FirstChild), out pVar);
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(hr);

            if (VSConstants.S_OK == hr)
            {
                uint siblingId = GetItemId(pVar);
                while (siblingId != VSConstants.VSITEMID_NIL)
                {
                    yield return new VsSolutionHierarchyNode(parent.VsHierarchy, siblingId, new System.Lazy<VsSolutionHierarchyNode>(() => parent));

                    hr = parent.VsHierarchy.GetProperty(siblingId, (int)(isSolution ? __VSHPROPID.VSHPROPID_NextVisibleSibling : __VSHPROPID.VSHPROPID_NextSibling), out pVar);

                    if (VSConstants.S_OK == hr)
                    {
                        siblingId = GetItemId(pVar);
                    }
                    else
                    {
                        Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(hr);
                        break;
                    }
                }
            }
        }

        private static uint GetItemId(object pvar)
        {
            if (pvar == null) return VSConstants.VSITEMID_NIL;
            if (pvar is int) return (uint)(int)pvar;
            if (pvar is uint) return (uint)pvar;
            if (pvar is short) return (uint)(short)pvar;
            if (pvar is ushort) return (uint)(ushort)pvar;
            if (pvar is long) return (uint)(long)pvar;
            return VSConstants.VSITEMID_NIL;
        }
    }
}