using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using Clide.Properties;
using Microsoft.Build.Evaluation;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide
{
	class ConfigProjectProperties : DynamicObject, IPropertyAccessor
	{
		static readonly ITracer tracer = Tracer.Get<ConfigProjectProperties>();

		ProjectNode project;
		IVsBuildPropertyStorage vsBuild;
		string configName;
		DynamicPropertyAccessor accessor;

		public ConfigProjectProperties(ProjectNode project, string configName)
		{
			this.project = project;
			this.configName = configName;
			vsBuild = project.HierarchyNode.HierarchyIdentity.Hierarchy as IVsBuildPropertyStorage;
			if (vsBuild == null)
				tracer.Warn(Strings.ConfigProjectProperties.NonMsBuildProject(project.Text));

			accessor = new DynamicPropertyAccessor(this);
		}

		public override IEnumerable<string> GetDynamicMemberNames()
		{
			var msb = project.As<Project>();
			if (msb != null)
			{
				return msb.AllEvaluatedProperties
					.Select(prop => prop.Name)
					.Distinct()
					.OrderBy(s => s);
			}

			return Enumerable.Empty<string>();
		}

		public override bool TryGetMember (GetMemberBinder binder, out object result) => accessor.TryGetMember (binder, out result, base.TryGetMember);

		public override bool TrySetMember (SetMemberBinder binder, object value) => accessor.TrySetMember (binder, value, base.TrySetMember);

		public override bool TryGetIndex (GetIndexBinder binder, object[] indexes, out object result) => accessor.TryGetIndex (binder, indexes, out result, base.TryGetIndex);

		public override bool TrySetIndex (SetIndexBinder binder, object[] indexes, object value) => accessor.TrySetIndex (binder, indexes, value, base.TrySetIndex);

		bool IPropertyAccessor.TrySetProperty(string propertyName, object value)
		{
			if (vsBuild != null)
			{
				return ErrorHandler.Succeeded(vsBuild.SetPropertyValue(
					propertyName, this.configName, (uint)_PersistStorageType.PST_PROJECT_FILE, value.ToString()));
			}
			else
			{
				tracer.Warn(Strings.ConfigProjectProperties.SetNonMsBuildProject(propertyName, configName, project.Text));
			}

			// In this case we fail, since we can't persist the member.
			return false;
		}

		bool IPropertyAccessor.TryGetProperty(string propertyName, out object result)
		{
			if (vsBuild != null)
			{
				string value = "";
				if (ErrorHandler.Succeeded(vsBuild.GetPropertyValue(
					propertyName, configName, (uint)_PersistStorageType.PST_PROJECT_FILE, out value)))
				{
					result = value;
					return true;
				}
			}

			// We always succeed, but return null. This 
			// is easier for the calling code than catching 
			// a binder exception.
			result = null;
			return true;
		}
	}
}
