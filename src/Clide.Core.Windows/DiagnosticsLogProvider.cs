using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Build;

namespace Clide
{
    [Export(typeof(IBuildLoggerProviderAsync))]
    [AppliesTo(".NET + XamarinForms")]
    internal class DiagnosticsLogProvider : IBuildLoggerProviderAsync
    {
        readonly UnconfiguredProject project;
        readonly DiagnosticsLogging logging;

        [ImportingConstructor]
        public DiagnosticsLogProvider(UnconfiguredProject project, DiagnosticsLogging logging)
        {
            this.project = project;
            this.logging = logging;
        }

        public Task<IImmutableSet<ILogger>> GetLoggersAsync(IReadOnlyList<string> targets, IImmutableDictionary<string, string> properties, CancellationToken cancellationToken)
        {
            if (!logging.ShouldLog)
                return Task.FromResult<IImmutableSet<ILogger>>(ImmutableHashSet<ILogger>.Empty);

            return Task.FromResult<IImmutableSet<ILogger>>(ImmutableHashSet<ILogger>.Empty.Add(logging.CreateLogger(project.FullPath)));
        }
    }
}
