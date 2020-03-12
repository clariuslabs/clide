using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;

namespace Clide
{
    /// <summary>
    /// Showcases a dynamic component that is loaded/unloaded as a capability is added or removed. 
    /// 
    /// The capability can be defined in the .csproj like
    /// 
    /// <ProjectCapability Include="DynamicCapability"  />
    /// 
    /// Also, the capability can even be conditioned to the existence of a given file, say:
    /// 
    /// <ProjectCapability Include="AndroidManifest" Condition="Exists(Properties\AndroidManifest.xml)" />
    /// </summary>
    [Export(ExportContractNames.Scopes.UnconfiguredProject, typeof(IProjectDynamicLoadComponent))]
    [AppliesTo("DynamicCapability")]
    internal class DynamicCapabilityExample : IProjectDynamicLoadComponent
    {
        public Task LoadAsync()
        {
            Debug.WriteLine("DynamicCapability enabled!");
            return Task.CompletedTask;
        }

        public Task UnloadAsync()
        {
            Debug.WriteLine("DynamicCapability disabled!");
            return Task.CompletedTask;
        }
    }
}
