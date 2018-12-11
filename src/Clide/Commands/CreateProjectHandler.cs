using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Merq;

namespace Clide
{
    [Export(typeof(ICommandHandler<CreateProjectCommand, IEnumerable<string>>))]
    [Export(typeof(ICanExecute<CreateProjectCommand>))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class CreateProjectHandler : ICommandHandler<CreateProjectCommand, IEnumerable<string>>
    {
        readonly Action<string> dotnetRunner;

        public bool CanExecute(CreateProjectCommand command) =>
            !string.IsNullOrEmpty(command.Template) && !string.IsNullOrEmpty(command.Output);

        public CreateProjectHandler()
            : this(command => RunDotNetCommand(command))
        { }

        internal CreateProjectHandler(Action<string> dotnetRunner) /* for unit testing */
        {
            this.dotnetRunner = dotnetRunner;
        }

        public IEnumerable<string> Execute(CreateProjectCommand command)
        {
            if (CanExecute(command))
            {
                var dotNetCommands = new string[]
                {
                    GetInstallCommandString(command),
                    GetUnfoldCommandString(command)
                };

                var timestamp = DateTime.UtcNow;

                dotnetRunner(string.Join(" && ", dotNetCommands.Where(x => !string.IsNullOrWhiteSpace(x))));

                if (Directory.Exists(command.Output))
                {
                    var projects = (from directory in new string[] { command.Output }.Concat(Directory.EnumerateDirectories(command.Output))
                           from projectFile in Directory.EnumerateFiles(directory, "*.csproj")
                           where File.GetLastWriteTimeUtc(projectFile) > timestamp
                           select projectFile).ToList();

                    return projects;
                }
            }

            return Enumerable.Empty<string>();
        }

        static void RunDotNetCommand(string command)
        {
            var startInfo = new ProcessStartInfo("cmd", "/c " + command)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true
            };

            var process = Process.Start(startInfo);
            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new Exception(process.StandardError.ReadToEnd());
        }

        string GetInstallCommandString(CreateProjectCommand command)
        {
            var result = default(string);

            if (!string.IsNullOrEmpty(command.Install))
            {
                result = $"dotnet new -i {command.Install}";
                if (!string.IsNullOrEmpty(command.NuGetSource))
                    result += $" --nuget-source \"{command.NuGetSource}\"";
            }

            return result;
        }

        string GetUnfoldCommandString(CreateProjectCommand command)
        {
            var options = new List<string>()
            {
                command.Template,
                $"-o \"{command.Output}\""
            };

            if (!string.IsNullOrEmpty(command.Language))
                options.Add($"-lang {command.Language}");

            if (command.Force)
                options.Add("--force");

            if (!string.IsNullOrEmpty(command.Name))
                options.Add($"-n {command.Name}");

            if (command.AdditionalOptions != null)
                options.AddRange(command.AdditionalOptions.Select(x => $"--{x.Key} \"{x.Value}\""));

            return $"dotnet new {string.Join(" ", options)}";
        }
    }
}