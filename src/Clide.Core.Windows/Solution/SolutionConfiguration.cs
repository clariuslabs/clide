using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;

namespace Clide
{
    class SolutionConfiguration : ISolutionConfiguration
    {
        readonly Lazy<Solution> solution;

        public SolutionConfiguration(Lazy<Solution> solution)
        {
            this.solution = solution;
        }

        public async Task ChangePlatformAsync(string platform)
        {
            await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var activeConfiguration = solution.Value.SolutionBuild.ActiveConfiguration as SolutionConfiguration2;

            if (activeConfiguration != null && activeConfiguration.PlatformName != platform)
            {
                var configurations = solution.Value.SolutionBuild.SolutionConfigurations;
                if (solution.Value.SolutionBuild.SolutionConfigurations != null)
                {
                    var targetConfiguration = solution.Value.SolutionBuild.SolutionConfigurations
                        .OfType<SolutionConfiguration2>()
                        .FirstOrDefault(x => x.Name == activeConfiguration.Name && x.PlatformName == platform);

                    if (targetConfiguration != null)
                    {
                        targetConfiguration.Activate();
                    }
                }
            }
        }
    }
}
