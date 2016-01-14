using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Clide
{
    internal class ProjectConfiguration : IProjectConfiguration
    {
        private ProjectNode project;

        public ProjectConfiguration(ProjectNode project)
        {
            this.project = project;
        }

        public string ActiveConfigurationName
        {
            get { return ActiveConfiguration + "|" + ActivePlatform; }
        }

        public string ActiveConfiguration
        {
            get { return project.Project.Value.ConfigurationManager.ActiveConfiguration.ConfigurationName; }
        }

        public string ActivePlatform
        {
            get { return project.Project.Value.ConfigurationManager.ActiveConfiguration.PlatformName.Replace(" ", "").Trim(); }
        }

        public IEnumerable<string> Configurations
        {
            get { return ((IEnumerable)project.Project.Value.ConfigurationManager.ConfigurationRowNames).OfType<string>(); }
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
