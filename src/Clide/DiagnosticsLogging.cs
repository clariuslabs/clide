using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using EnvDTE;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
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

        static readonly string LogsBaseDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Xamarin", "Logs");

        static readonly bool shouldLog = (Environment.GetCommandLineArgs() ?? Array.Empty<string>())
            .Any(x => "/log".Equals(x, StringComparison.OrdinalIgnoreCase));

        readonly JoinableTaskFactory jtf;
        readonly JoinableTask<string> vsVersion;

        [ImportingConstructor]
        public DiagnosticsLogging(
            [Import] JoinableTaskContext jtc,
            [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            jtf = jtc.Factory;
            vsVersion = jtf.RunAsync(async () =>
            {
                await jtf.SwitchToMainThreadAsync();
                return serviceProvider.GetService<DTE>().Version;
            });
        }

        public bool ShouldLog => shouldLog;

        public ILogger CreateLogger(string projectPath)
        {
            string version = default;
            if (!vsVersion.IsCompleted)
                version = jtf.Run(async () => await vsVersion);
            else
                version = vsVersion.Task.Result;

            var logFile = Path.Combine(
                LogsBaseDir,
                version,
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
