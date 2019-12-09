using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clide.Sdk;
using Microsoft.VisualStudio.Shell;

namespace Clide.Adapters
{
    [Adapter]
    class SolutionNodeToDte : IAdapter<SolutionNode, EnvDTE.Solution>
    {
        public EnvDTE.Solution Adapt(SolutionNode from) {
            return ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                var dte = ServiceLocator.Global.GetService<EnvDTE.DTE>();

                return dte.Solution;
            });
        }
    }
}
