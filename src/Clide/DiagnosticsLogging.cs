using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using EnvDTE;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using Microsoft.Internal.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;

namespace Clide
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class DiagnosticsLogging
    {
        /// {date_time}.{process}.{project}.binlog
        /// </summary>
        const string FileNameFormat = "{0}.{1}.{2}.binlog";

        readonly JoinableTaskFactory jtf;
        readonly JoinableTask initialize;
        string logsDir;

        [ImportingConstructor]
        public DiagnosticsLogging(
            [Import] JoinableTaskContext jtc,
            [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            jtf = jtc.Factory;
            initialize = jtf.RunAsync(async () =>
            {
                await jtf.SwitchToMainThreadAsync();

                logsDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Xamarin", "Logs", serviceProvider.GetService<DTE>().Version);

                ShouldLog = !serviceProvider.GetService<SVsFeatureFlags, IVsFeatureFlags>().IsFeatureEnabled("Xamarin.DisableDiagnosticsBinlog", false) &&
                    (Environment.GetCommandLineArgs() ?? Array.Empty<string>()).Any(x => "/log".Equals(x, StringComparison.OrdinalIgnoreCase));
            });
        }

        public bool ShouldLog { get; private set; }

        public ILogger CreateLogger(string projectPath)
        {
            if (!initialize.IsCompleted)
                jtf.Run(async () => await initialize);

            var logFile = Path.Combine(
                logsDir,
                string.Format(
                    FileNameFormat,
                    DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"),
                    System.Diagnostics.Process.GetCurrentProcess().Id,
                    Path.GetFileNameWithoutExtension(projectPath ?? "")));

            return new BinaryLogger
            {
                Parameters = logFile,
                Verbosity = LoggerVerbosity.Diagnostic,
                CollectProjectImports = BinaryLogger.ProjectImportsCollectionMode.None
            };
        }
    }
}
