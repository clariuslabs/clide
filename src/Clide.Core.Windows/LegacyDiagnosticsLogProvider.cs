using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Build;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.BuildLogging;

namespace Clide
{
    [AppliesTo("Managed + (Android | iOS | XamarinXaml)")]
    [Export("Xamarin.VisualStudio.BuildLoggerProvider", typeof(IVsBuildLoggerProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class LegacyDiagnosticsLogProvider : IVsBuildLoggerProvider
    {
        static readonly Lazy<BuildLoggerEvents> allEvents = new Lazy<BuildLoggerEvents>(() => Enum
            .GetValues(typeof(BuildLoggerEvents))
            .Cast<BuildLoggerEvents>()
            .Aggregate((BuildLoggerEvents)0, (result, current) => result |= current));

        DiagnosticsLogging logging;

        [ImportingConstructor]
        public LegacyDiagnosticsLogProvider(DiagnosticsLogging logging) => this.logging = logging;

        public LoggerVerbosity Verbosity => LoggerVerbosity.Diagnostic;

        public BuildLoggerEvents Events => allEvents.Value;

        public ILogger GetLogger(string projectPath, IEnumerable<string> targets, IDictionary<string, string> properties, bool isDesignTimeBuild)
        {
            if (!logging.ShouldLog)
                return null;

            return logging.CreateLogger(projectPath);
        }
    }
}
