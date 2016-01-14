using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Clide
{
	/// <summary>
	/// Common behaviors associated with <see cref="IVsHierarchyItem"/>.
	/// </summary>
	[DebuggerDisplay ("{DisplayName}")]
	internal class VsSolutionItemNode : IVsSolutionItemNode
	{
		Lazy<object> extensibilityObject;
		Lazy<IServiceProvider> serviceProvider;
		Lazy<VsSolutionItemNode> parent;
		//Lazy<string> name;
		//Lazy<string> caption;

		internal VsSolutionItemNode (IVsHierarchyItem item)
        {
			if (item.HierarchyIdentity.IsNestedItem) {
				VsHierarchy = item.HierarchyIdentity.NestedHierarchy;
				ItemId = item.HierarchyIdentity.NestedItemID;
			} else {
				VsHierarchy = item.HierarchyIdentity.Hierarchy;
				ItemId = item.HierarchyIdentity.ItemID;
			}

            extensibilityObject = new Lazy<object>(() => item.GetProperty((int)__VSHPROPID.VSHPROPID_ExtObject));
			serviceProvider = new Lazy<IServiceProvider> (() => item.GetServiceProvider ());
			//name = new Lazy<string> (() => item.GetProperty<string> ((int)__VSHPROPID.VSHPROPID_Name));
			//caption = new Lazy<string> (() => item.GetProperty<string> ((int)__VSHPROPID.VSHPROPID_Caption));

			parent = new Lazy<VsSolutionItemNode> (() => item.Parent == null ? null : new VsSolutionItemNode (item.Parent));
        }

		public IVsSolutionItemNode Parent { get { return parent.Value; } }

		public IEnumerable<IVsSolutionHierarchyNode> Children
		{
			get { return new VsSolutionHierarchyNodeIterator(this); }
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

        public override string ToString()
        {
            return this.DisplayName;
        }
	}
}