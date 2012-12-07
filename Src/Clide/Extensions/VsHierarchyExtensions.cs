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

namespace Clide
{
    using System;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
	/// Provides extensions for Visual Studio low-level <see cref="IVsHierarchy"/> API.
	/// </summary>
	internal static class VsHierarchyExtensions
	{
		internal static Func<IVsHierarchy, uint, IVsHierarchyProperties> PropertiesFactory = (hierarchy, itemId) => new VsHierarchyProperties(hierarchy, itemId);

		/// <summary>
		/// Provides access to the hierarchy properties for a specific child item.
		/// </summary>
		public static IVsHierarchyProperties Properties(this IVsHierarchy hierarchy, uint itemId)
		{
			return PropertiesFactory(hierarchy, itemId);
		}

		/// <summary>
		/// Provides access to the properties of the hierarchy root itself (i.e. the solution, a project, etc.).
		/// </summary>
		public static IVsHierarchyProperties Properties(this IVsHierarchy hierarchy)
		{
			return PropertiesFactory(hierarchy, GetItemId(hierarchy));
		}

		private static uint GetItemId(IVsHierarchy hierarchy)
		{
			object extObject;
			uint itemId = 0;
			IVsHierarchy tempHierarchy;

			ErrorHandler.ThrowOnFailure(
				hierarchy.GetProperty(
					VSConstants.VSITEMID_ROOT,
					(int)__VSHPROPID.VSHPROPID_BrowseObject,
					out extObject));

			var browseObject = extObject as IVsBrowseObject;
			if (browseObject != null)
				browseObject.GetProjectItem(out tempHierarchy, out itemId);

			return itemId;
		}

		private class VsHierarchyProperties : IVsHierarchyProperties
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

			public IVsHierarchy Parent
			{
				get { return GetProperty<IVsHierarchy>(hierarchy, __VSHPROPID.VSHPROPID_ParentHierarchy, VSConstants.VSITEMID_ROOT); }
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

			private static T GetProperty<T>(IVsHierarchy hierarchy, __VSHPROPID propId)
			{
				return GetProperty<T>(hierarchy, propId, GetItemId(hierarchy));
			}
		}
	}
}
