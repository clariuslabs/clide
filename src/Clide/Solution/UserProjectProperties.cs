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
            msBuildProject = project.AsMsBuildProject();
            dteProject = project.As<EnvDTE.Project>();
            vsBuild = project.AsVsHierarchy() as IVsBuildPropertyStorage;

            if (msBuildProject == null || vsBuild == null)
                tracer.Warn(Strings.UserProjectProperties.NonMsBuildProject(project.Text));

            accessor = new DynamicPropertyAccessor(this);
        }

        // Enumeration is not supported by the underlying VS API.
        public override IEnumerable<string> GetDynamicMemberNames() => Enumerable.Empty<string>();

        public override bool TryGetMember(GetMemberBinder binder, out object result) => accessor.TryGetMember(binder, out result, base.TryGetMember);

        public override bool TrySetMember(SetMemberBinder binder, object value) => accessor.TrySetMember(binder, value, base.TrySetMember);

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result) => accessor.TryGetIndex(binder, indexes, out result, base.TryGetIndex);

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value) => accessor.TrySetIndex(binder, indexes, value, base.TrySetIndex);

        bool IPropertyAccessor.TryGetProperty(string propertyName, out object result)
        {
            if (vsBuild != null)
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
                    var configName = dteProject.ConfigurationManager.ActiveConfiguration.ConfigurationName + "|" +
                        dteProject.ConfigurationManager.ActiveConfiguration.PlatformName;

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
            if (vsBuild != null)
            {
                if (ErrorHandler.Succeeded(vsBuild.SetPropertyValue(
                    propertyName, "", (uint)_PersistStorageType.PST_USER_FILE, value.ToString())))
                    return true;
            }
            else
            {
                tracer.Warn(Strings.UserProjectProperties.SetNonMsBuildProject(propertyName, project.Text));
            }

            // In this case we fail, since we can't persist the member.
            return false;
        }
    }
}