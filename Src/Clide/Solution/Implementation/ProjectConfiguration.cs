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
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal class ProjectConfiguration : IProjectConfiguration
    {
        private ProjectNode project;

        public ProjectConfiguration(ProjectNode project)
        {
            this.project = project;
        }

        public string ActiveConfigurationName
        {
            get { return this.ActiveConfiguration + "|" + this.ActivePlatform; }
        }

        public string ActiveConfiguration
        {
            get { return this.project.Project.Value.ConfigurationManager.ActiveConfiguration.ConfigurationName; }
        }

        public string ActivePlatform
        {
            get { return this.project.Project.Value.ConfigurationManager.ActiveConfiguration.PlatformName.Replace(" ", "").Trim(); }
        }

        public IEnumerable<string> Configurations
        {
            get { return ((IEnumerable)this.project.Project.Value.ConfigurationManager.ConfigurationRowNames).OfType<string>(); }
        }

        public IEnumerable<string> Platforms
        {
            get
            {
                return ((IEnumerable)this.project.Project.Value.ConfigurationManager.PlatformNames)
                    .OfType<string>()
                    // The configuration API does not use whitespaces.
                    .Select(s => s.Replace(" ", "").Trim());
            }
        }
    }
}
