using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Microsoft.Build.Evaluation;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide
{
    class GlobalProjectProperties : DynamicObject, IPropertyAccessor
    {
        Project msBuildProject;
        EnvDTE.Project dteProject;
        IVsBuildPropertyStorage vsBuild;
        DynamicPropertyAccessor accessor;

        public GlobalProjectProperties(ProjectNode project)
        {
            msBuildProject = project.As<Project>();
            dteProject = project.As<EnvDTE.Project>();
            vsBuild = project.AsVsBuildPropertyStorage();
            accessor = new DynamicPropertyAccessor(this);
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            var names = new List<string>();

            if (dteProject != null)
            {
                names.AddRange(dteProject.Properties
                    .OfType<EnvDTE.Property>()
                    .Select(prop => prop.Name));
            }

            if (msBuildProject != null)
            {
                names.AddRange(msBuildProject.AllEvaluatedProperties
                    .Select(prop => prop.Name));
            }

            names.Sort();
            return names.Distinct();
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result) => accessor.TryGetMember(binder, out result, base.TryGetMember);

        public override bool TrySetMember(SetMemberBinder binder, object value) => accessor.TrySetMember(binder, value, base.TrySetMember);

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result) => accessor.TryGetIndex(binder, indexes, out result, base.TryGetIndex);

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value) => accessor.TrySetIndex(binder, indexes, value, base.TrySetIndex);

        bool IPropertyAccessor.TryGetProperty(string propertyName, out object result)
        {
            if (TryGetDteProperty(propertyName, out result))
                return true;

            if (vsBuild != null)
            {
                string value = "";
                if (ErrorHandler.Succeeded(vsBuild.GetPropertyValue(
                    propertyName, "", (uint)_PersistStorageType.PST_PROJECT_FILE, out value)))
                {
                    result = value;
                    return true;
                }

                if (msBuildProject != null)
                {
                    var configName = dteProject.ConfigurationManager.ActiveConfiguration.ConfigurationName + "|" +
                        dteProject.ConfigurationManager.ActiveConfiguration.PlatformName;

                    if (ErrorHandler.Succeeded(vsBuild.GetPropertyValue(
                        propertyName, configName, (uint)_PersistStorageType.PST_PROJECT_FILE, out value)))
                    {
                        result = value;
                        return true;
                    }

                    var prop = msBuildProject.GetProperty(propertyName);
                    if (prop != null)
                    {
                        result = prop.EvaluatedValue;
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
            if (TrySetDteProperty(propertyName, value))
                return true;

            if (vsBuild != null)
            {
                if (ErrorHandler.Succeeded(vsBuild.SetPropertyValue(
                    propertyName, "", (uint)_PersistStorageType.PST_PROJECT_FILE, value.ToString())))
                    return true;
            }

            if (msBuildProject != null)
            {
                msBuildProject.SetProperty(propertyName, value.ToString());
                return true;
            }

            // In this case we fail, since we can't persist the member.
            return false;
        }

        bool TrySetDteProperty(string propertyName, object value)
        {
            if (dteProject != null)
            {
                EnvDTE.Property property;
                try
                {
                    property = dteProject.Properties.Item(propertyName);
                }
                catch (ArgumentException)
                {
                    property = null;
                }
                if (property != null)
                {
                    property.Value = value.ToString();
                    return true;
                }
            }

            return false;
        }

        bool TryGetDteProperty(string propertyName, out object result)
        {
            if (dteProject != null)
            {
                EnvDTE.Property property;
                try
                {
                    property = dteProject.Properties.Item(propertyName);
                }
                catch (ArgumentException)
                {
                    property = null;
                }
                if (property != null)
                {
                    result = property.Value;
                    return true;
                }
            }

            result = null;
            return false;
        }
    }
}
