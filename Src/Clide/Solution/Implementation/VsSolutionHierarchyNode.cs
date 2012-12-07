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

namespace Clide.Solution
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using System.Diagnostics;
    using Microsoft.VisualStudio.Shell;

    /// <summary>
	/// A general-purpose <see cref="SolutionExplorerNode"/> that relies on 
	/// <see cref="IVsHierarchy"/> for its behavior and can be reused for most 
	/// project types.
	/// </summary>
	[DebuggerDisplay("{DisplayName}")]
	internal class VsSolutionHierarchyNode : IVsSolutionHierarchyNode
	{
		private Lazy<object> extensibilityObject;
		private Lazy<IServiceProvider> serviceProvider;

		internal VsSolutionHierarchyNode(IVsHierarchy hierarchy)
			: this(hierarchy, hierarchy.Properties().ItemId)
		{
		}

		internal VsSolutionHierarchyNode(IVsHierarchy hierarchy, uint itemId)
		{
			Guard.NotNull(() => hierarchy, hierarchy);

			this.VsHierarchy = hierarchy;
			this.ItemId = itemId;

			IntPtr nestedHierarchyObj;
			uint nestedItemId;
			Guid hierGuid = typeof(IVsHierarchy).GUID;

			int hr = hierarchy.GetNestedHierarchy(this.ItemId, ref hierGuid, out nestedHierarchyObj, out nestedItemId);
			if (VSConstants.S_OK == hr && IntPtr.Zero != nestedHierarchyObj)
			{
				IVsHierarchy nestedHierarchy = Marshal.GetObjectForIUnknown(nestedHierarchyObj) as IVsHierarchy;
				Marshal.Release(nestedHierarchyObj);
				if (nestedHierarchy != null)
				{
					this.VsHierarchy = nestedHierarchy;
					this.ItemId = nestedItemId;
				}
			}

			this.extensibilityObject = new Lazy<object>(() => this.VsHierarchy.Properties().ExtenderObject);
            this.serviceProvider = new Lazy<IServiceProvider>(() =>
            {
                Microsoft.VisualStudio.OLE.Interop.IServiceProvider oleSp;
                hierarchy.GetSite(out oleSp);
                return oleSp != null ? 
                    new ServiceProvider(oleSp) : 
                    Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider;
            });

			this.DisplayName = this.VsHierarchy.Properties(this.ItemId).DisplayName;
		}

		public string DisplayName { get; private set; }

		public IVsSolutionHierarchyNode Parent
		{
			get
			{
				var parentHierarchy = this.VsHierarchy.Properties().Parent;
				if (parentHierarchy == null)
				{
					return null;
				}

				return new VsSolutionHierarchyNode(parentHierarchy);
			}
		}

		public IEnumerable<IVsSolutionHierarchyNode> Children
		{
			get { return new VsSolutionHierarchyNodeIterator(this.VsHierarchy, this.ItemId); }
		}

		public IVsHierarchy VsHierarchy { get; private set; }
		public uint ItemId { get; private set; }

		public object ExtensibilityObject
		{
			get { return this.extensibilityObject.Value; }
		}

		public IServiceProvider ServiceProvider
		{
			get { return this.serviceProvider.Value; }
		}
	}
}
