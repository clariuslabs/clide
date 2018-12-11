using System;
using System.Linq;
using System.Collections.Generic;
using EnvDTE;
using Merq;
using Microsoft.VisualStudio.TemplateWizard;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;

namespace Clide
{
    public class CliTemplateEngineWizard : IWizard
    {
        const string PassthroughParameterPrefix = "$passthrough:";
        const string PassthroughParameterSuffix = "$";

        readonly ICommandBus commandBus;
        readonly JoinableLazy<IVsSolution> vsSolution;
        Dictionary<string, string> replacementsDictionary;

        public CliTemplateEngineWizard()
            : this(ServiceLocator.Global.GetInstance<ICommandBus>(), ServiceLocator.Global.GetInstance<JoinableLazy<IVsSolution>>())
        { }

        public CliTemplateEngineWizard(ICommandBus commandBus, JoinableLazy<IVsSolution> vsSolution)
        {
            this.commandBus = commandBus;
            this.vsSolution = vsSolution;
        }

        public void BeforeOpeningFile(ProjectItem projectItem) { }

        public void ProjectFinishedGenerating(Project project) { }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem) { }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            this.replacementsDictionary = replacementsDictionary;
        }

        public void RunFinished()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var createdProjects = commandBus.Execute(
                new CreateProjectCommand
                {
                    Template = GetReplacementValue("$template$"),
                    Output = string.IsNullOrEmpty(GetReplacementValue("$specifiedsolutionname$")) ? GetReplacementValue("$solutiondirectory$") : GetReplacementValue("$destinationdirectory$"),
                    Force = string.Equals("true", GetReplacementValue("$force$"), StringComparison.OrdinalIgnoreCase),
                    Language = GetReplacementValue("$language$"),
                    Name = GetReplacementValue("$projectname$"),
                    Install = GetReplacementValue("$install$"),
                    NuGetSource = GetReplacementValue("$nugetsource$"),
                    AdditionalOptions = replacementsDictionary
                    .Where(x => x.Key.StartsWith(PassthroughParameterPrefix) && x.Key.EndsWith(PassthroughParameterSuffix))
                    .Select(x => new KeyValuePair<string, string>(
                        x.Key.Substring(PassthroughParameterPrefix.Length, x.Key.Length - PassthroughParameterPrefix.Length - PassthroughParameterSuffix.Length),
                        x.Value))
                    .ToDictionary(x => x.Key, x => x.Value)
                });

            foreach (var projectFile in createdProjects)
                vsSolution.GetValue().CreateProject(Guid.Empty, projectFile, null, null, (uint)(__VSCREATEPROJFLAGS.CPF_OPENFILE), Guid.Empty, out var project);
        }

        public bool ShouldAddProjectItem(string filePath) => false;

        string GetReplacementValue(string key) =>
            replacementsDictionary.TryGetValue(key, out var value) ? value : null;
    }
}