using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.VisualStudio.ProjectSystem;

namespace Clide
{
    /// <summary>
    /// Shows how to implement code that runs on project loaded and before unloading.
    /// </summary>
    class CapabilitiesMonitoringExample
    {
        IDisposable subscription;

        [Import]
        UnconfiguredProject project;

        [AppliesTo(ProjectCapabilities.Managed)]
        [ProjectAutoLoad(ProjectLoadCheckpoint.AfterLoadInitialConfiguration)]
        public Task OnProjectLoadedAsync()
        {
            subscription = project.Capabilities.SourceBlock.LinkTo(
                DataflowBlockSlim.CreateActionBlock<IProjectVersionedValue<IProjectCapabilitiesSnapshot>>(CapabilitiesUpdated));

            project.ProjectUnloading += OnProjectUnloadingAsync;

            return Task.CompletedTask;
        }

        void CapabilitiesUpdated(IProjectVersionedValue<IProjectCapabilitiesSnapshot> value)
        {
            Debug.WriteLine($"Supports DynamicCapability: {value.Value.IsProjectCapabilityPresent("DynamicCapability")}");

            // NOTE: cannot as for composite capabilities, like 'Managed + CSharp'

            Debug.WriteLine($"Supports Managed: {value.Value.IsProjectCapabilityPresent("Managed")}");
            Debug.WriteLine($"Supports C#: {value.Value.IsProjectCapabilityPresent(ProjectCapabilities.CSharp)}");
        }

        Task OnProjectUnloadingAsync(object sender, EventArgs args)
        {
            project.ProjectUnloading -= OnProjectUnloadingAsync;
            
            // Dispose subscription to stop getting updates.
            subscription.Dispose();
            
            return Task.CompletedTask;
        }
    }
}
