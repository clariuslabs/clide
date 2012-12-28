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

namespace Clide.Solution
{
    using Microsoft.Build.Evaluation;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal class GlobalProjectProperties : DynamicObject
    {
        private Project msBuildProject;
        private EnvDTE.Project dteProject;
        private IVsBuildPropertyStorage vsBuild;

        public GlobalProjectProperties(ProjectNode project)
        {
            this.msBuildProject = project.As<Project>();
            this.dteProject = project.As<EnvDTE.Project>();
            this.vsBuild = project.HierarchyNode.VsHierarchy as IVsBuildPropertyStorage;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            var names = new List<string>();

            if (this.dteProject != null)
            {
                names.AddRange(this.dteProject.Properties
                    .OfType<EnvDTE.Property>()
                    .Select(prop => prop.Name));
            }

            if (this.msBuildProject != null)
            {
                names.AddRange(this.msBuildProject.AllEvaluatedProperties
                    .Select(prop => prop.Name));
            }

            names.Sort();
            return names.Distinct();
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (TryGetDteProperty(binder.Name, out result))
                return true;

            if (this.vsBuild != null)
            {
                string value = "";
                if (ErrorHandler.Succeeded(vsBuild.GetPropertyValue(
                    binder.Name, "", (uint)_PersistStorageType.PST_PROJECT_FILE, out value)))
                {
                    result = value;
                    return true;
                }
            }
            
            if (msBuildProject != null)
            {
                string value = "";
                var configName = this.dteProject.ConfigurationManager.ActiveConfiguration.ConfigurationName + "|" +
                    this.dteProject.ConfigurationManager.ActiveConfiguration.PlatformName;

                if (ErrorHandler.Succeeded(vsBuild.GetPropertyValue(
                    binder.Name, configName, (uint)_PersistStorageType.PST_PROJECT_FILE, out value)))
                {
                    result = value;
                    return true;
                }

                var prop = msBuildProject.GetProperty(binder.Name);
                if (prop != null)
                {
                    result = prop.EvaluatedValue;
                    return true;
                }
            }

            // We always succeed, but return null. This 
            // is easier for the calling code than catching 
            // a binder exception.
            result = null;
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (TrySetDteProperty(binder.Name, value))
                return true;

            if (this.vsBuild != null)
            {
                if (ErrorHandler.Succeeded(vsBuild.SetPropertyValue(
                    binder.Name, "", (uint)_PersistStorageType.PST_PROJECT_FILE, value.ToString())))
                    return true;
            }
            
            if (this.msBuildProject != null)
            {
                this.msBuildProject.SetProperty(binder.Name, value.ToString());
                return true;
            }

            // In this case we fail, since we can't persist the member.
            return false;
        }

        private bool TrySetDteProperty(string propertyName, object value)
        {
            if (this.dteProject != null)
            {
                EnvDTE.Property property;
                try
                {
                    property = this.dteProject.Properties.Item(propertyName);
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

        private bool TryGetDteProperty(string propertyName, out object result)
        {
            if (this.dteProject != null)
            {
                EnvDTE.Property property;
                try
                {
                    property = this.dteProject.Properties.Item(propertyName);
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
