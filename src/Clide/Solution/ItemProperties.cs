using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Clide.Properties;

namespace Clide
{
	internal class ItemProperties : DynamicObject
	{
		static readonly ITracer tracer = Tracer.Get<ItemProperties>();

		IVsHierarchyItem node;
		ProjectItem item;
		private IVsBuildPropertyStorage msBuild;

		public ItemProperties (ItemNode item)
		{
			this.item = item.HierarchyNode.GetExtenderObject () as ProjectItem;
			node = item.HierarchyNode;
			msBuild = item.HierarchyNode.GetRoot ().HierarchyIdentity.Hierarchy as IVsBuildPropertyStorage;
		}

		public override IEnumerable<string> GetDynamicMemberNames ()
		{
			return GetPropertyNames ();
		}

		public override bool TrySetMember (SetMemberBinder binder, object value)
		{
			return SetValue (binder.Name, value);
		}

		public override bool TryGetMember (GetMemberBinder binder, out object result)
		{
			result = GetValue (binder.Name);
			return true;
		}

		public object GetValue (string name)
		{
			var value = default(string);

			if (item != null) {
				Property property;
				try {
					property = item.Properties.Item (name);
				} catch (ArgumentException) {
					property = null;
				}

				if (property != null)
					return property.Value;
			}

			if (msBuild != null)
				msBuild.GetItemAttribute (node.HierarchyIdentity.ItemID, name, out value);

			return value;
		}

		public bool SetValue (string name, object value)
		{
			if (value == null)
				throw new ArgumentException (Strings.ItemProperties.InvalidNullValue(name), "value");

			if (item != null) {
				Property property;
				try {
					property = this.item.Properties.Item (name);
				} catch (ArgumentException) {
					property = null;
				}

				if (property != null) {
					try {
						property.Value = value;
						return true;
					} catch {
						return false;
					}
				}
			}

			// Fallback to MSBuild item properties.
			if (msBuild != null) {
				return ErrorHandler.Succeeded (
					this.msBuild.SetItemAttribute (node.HierarchyIdentity.ItemID, name, value.ToString ()));
			}

			return false;
		}

		IEnumerable<string> GetPropertyNames ()
		{
			try {
				return ((ProjectItem)node.GetExtenderObject())
					.Properties
					.Cast<Property> ()
					.Select (prop => prop.Name);
			} catch {
				return Enumerable.Empty<string> ();
			}
		}
	}
}
