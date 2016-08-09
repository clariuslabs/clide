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

namespace Clide.Solution.Implementation
{
	using Clide.Diagnostics;
	using Clide.Properties;
	using Clide.Sdk.Solution;
	using Microsoft.Build.Evaluation;
	using Microsoft.VisualStudio;
	using Microsoft.VisualStudio.Shell.Interop;
	using System;
	using System.Collections.Generic;
	using System.Dynamic;
	using System.Linq;
	using System.Diagnostics;

	class UserProjectProperties : DynamicObject, IPropertyAccessor
	{
		static readonly ITracer tracer = Tracer.Get<UserProjectProperties>();

		ProjectNode project;
		Project msBuildProject;
		EnvDTE.Project dteProject;
		IVsBuildPropertyStorage vsBuild;
		DynamicPropertyAccessor accessor;

		public UserProjectProperties(ProjectNode project)
		{
			this.project = project;
			msBuildProject = project.As<Project>();
			dteProject = project.As<EnvDTE.Project>();
			vsBuild = project.HierarchyNode.VsHierarchy as IVsBuildPropertyStorage;

			if (msBuildProject == null || vsBuild == null)
				tracer.Warn(Strings.UserProjectProperties.NonMsBuildProject(project.DisplayName));

			accessor = new DynamicPropertyAccessor(this);
		}

		public override IEnumerable<string> GetDynamicMemberNames()
		{
			// Enumeration is not supported by the underlying VS API.
			return Enumerable.Empty<string>();
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			return accessor.TryGetMember(binder, out result, base.TryGetMember);
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			return accessor.TrySetMember(binder, value, base.TrySetMember);
		}

		public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
		{
			return accessor.TryGetIndex(binder, indexes, out result, base.TryGetIndex);
		}

		public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
		{
			return accessor.TrySetIndex(binder, indexes, value, base.TrySetIndex);
		}

		bool IPropertyAccessor.TryGetProperty(string propertyName, out object result)
		{
			if (this.vsBuild != null)
			{
				string value = "";
				if (ErrorHandler.Succeeded(vsBuild.GetPropertyValue(
					propertyName, "", (uint)_PersistStorageType.PST_USER_FILE, out value)))
				{
					result = value;
					return true;
				}

				if (msBuildProject != null && dteProject != null)
				{
					var configName = this.dteProject.ConfigurationManager.ActiveConfiguration.ConfigurationName + "|" +
						this.dteProject.ConfigurationManager.ActiveConfiguration.PlatformName;

					if (ErrorHandler.Succeeded(vsBuild.GetPropertyValue(
						propertyName, configName, (uint)_PersistStorageType.PST_USER_FILE, out value)))
					{
						result = value;
						return true;
					}
				}
			}

			// We always succeed, but return null. This 
			// is easier for the calling code than catching 
			// a binder exception.
			result = null;
			return true;
		}

		bool IPropertyAccessor.TrySetProperty(string propertyName, object value)
		{
			if (this.vsBuild != null)
			{
				if (ErrorHandler.Succeeded(vsBuild.SetPropertyValue(
					propertyName, "", (uint)_PersistStorageType.PST_USER_FILE, value.ToString())))
					return true;
			}
			else
			{
				tracer.Warn(Strings.UserProjectProperties.SetNonMsBuildProject(propertyName, project.DisplayName));
			}

			// In this case we fail, since we can't persist the member.
			return false;
		}
	}
}