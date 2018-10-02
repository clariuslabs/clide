using EnvDTE;
using Microsoft.VisualStudio.Shell;
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

        public Awaitable<bool> IsDeployEnabled => Awaitable.Create(async () =>
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var solution = project.Value.DTE.Solution;

            if (solution.SolutionBuild.ActiveConfiguration != null)
            {
                var projectPath = project.Value.FullName;

                foreach (EnvDTE.SolutionContext context in solution.SolutionBuild.ActiveConfiguration.SolutionContexts)
                {
                    string projectName = context.ProjectName;
                    if (projectPath.EndsWith(projectName))
                    {
                        return context.ShouldDeploy;
                    }
                }
            }
            return false;
        });

        public Awaitable<bool> IsBuildEnabled => Awaitable.Create(async () =>
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var solution = project.Value.DTE.Solution;

            if (solution.SolutionBuild.ActiveConfiguration != null)
            {
                var projectPath = project.Value.FullName;

                foreach (EnvDTE.SolutionContext context in solution.SolutionBuild.ActiveConfiguration.SolutionContexts)
                {
                    string projectName = context.ProjectName;
                    if (projectPath.EndsWith(projectName))
                    {
                        return context.ShouldBuild;
                    }
                }
            }
            return false;
        });



    }
}
