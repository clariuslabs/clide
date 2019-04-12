using System;
using Microsoft.VisualStudio.Shell;

namespace Clide
{
    static class ISolutionExplorerExtensions
    {
        public static ISolutionNode GetSolution(this ISolutionExplorer solutionExplorer) =>
            ThreadHelper.JoinableTaskFactory.Run(async () => await solutionExplorer.Solution);
    }
}