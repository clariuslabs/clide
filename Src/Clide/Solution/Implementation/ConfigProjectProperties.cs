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
    using Clide.Properties;
    using Microsoft.Build.Evaluation;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal class ConfigProjectProperties : DynamicObject
    {
        private ProjectNode node;
        private IVsBuildPropertyStorage vsBuild;
        private string configName;

        public ConfigProjectProperties(ProjectNode project, string configName)
        {
            this.node = project;
            this.configName = configName;
            this.vsBuild = project.HierarchyNode.VsHierarchy as IVsBuildPropertyStorage;
            if (this.vsBuild == null)
                throw new NotSupportedException(
                    Strings.ConfigProjectProperties.UnsupportedProject(
                        Path.GetFileName(project.PhysicalPath)));
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            var msb = this.node.As<Project>();
            if (msb != null)
            {
                return msb.AllEvaluatedProperties
                    .Select(prop => prop.Name)
                    .Distinct()
                    .OrderBy(s => s);
            }

            return Enumerable.Empty<string>();
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (this.vsBuild != null)
            {
                string value = "";
                if (ErrorHandler.Succeeded(vsBuild.GetPropertyValue(
                    binder.Name, this.configName, (uint)_PersistStorageType.PST_PROJECT_FILE, out value)))
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

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (this.vsBuild != null)
            {
                return ErrorHandler.Succeeded(vsBuild.SetPropertyValue(
                    binder.Name, this.configName, (uint)_PersistStorageType.PST_PROJECT_FILE, value.ToString()));
            }

            // In this case we fail, since we can't persist the member.
            return false;
        }
    }
}
