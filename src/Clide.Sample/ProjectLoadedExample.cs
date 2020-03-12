using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;

namespace Clide
{
    /// <summary>
    /// Shows how to implement code that runs on project loaded and before unloading.
    /// </summary>
    class ProjectLoadedExample
    {
        [Import]
        UnconfiguredProject project;

        [AppliesTo(ProjectCapabilities.Managed + " + " + ProjectCapabilities.CSharp)]
        [ProjectAutoLoad(ProjectLoadCheckpoint.AfterLoadInitialConfiguration)]
        public Task OnProjectLoadedAsync()
        {
            project.ProjectUnloading += OnProjectUnloadingAsync;

            Debug.WriteLine($"Project {Path.GetFileName(project.FullPath)} loaded");

            return Task.CompletedTask;
        }

        Task OnProjectUnloadingAsync(object sender, EventArgs args)
        {
            project.ProjectUnloading -= OnProjectUnloadingAsync;

            Debug.WriteLine($"Project {Path.GetFileName(project.FullPath)} unloading");

            return Task.CompletedTask;
        }
    }
}
