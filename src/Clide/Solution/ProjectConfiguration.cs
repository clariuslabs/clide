using EnvDTE;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Clide
{

    internal class ProjectConfiguration : IProjectConfiguration
    {
        private Lazy<Project> project;

        public ProjectConfiguration(Lazy<Project> project)
        {
            this.project = project;
        }

        public string ActiveConfigurationName
        {
            get { return this.ActiveConfiguration + "|" + this.ActivePlatform; }
        }

        public string ActiveConfiguration
        {
            get { return project.Value.ConfigurationManager.ActiveConfiguration.ConfigurationName; }
        }

        public string ActivePlatform
        {
            get { return project.Value.ConfigurationManager.ActiveConfiguration.PlatformName.Replace(" ", "").Trim(); }
        }

        public IEnumerable<string> Configurations
        {
            get { return ((IEnumerable)project.Value.ConfigurationManager.ConfigurationRowNames).OfType<string>(); }
        }

        public IEnumerable<string> Platforms
        {
            get
            {
                return ((IEnumerable)project.Value.ConfigurationManager.PlatformNames)
                    .OfType<string>()
                    // The configuration API does not use whitespaces.
                    .Select(s => s.Replace(" ", "").Trim());
            }
        }
    }
}
