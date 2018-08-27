using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

[assembly: ProvideCodeBase]
[assembly: ProvideCodeBase(AssemblyName = "Clide.Core")]
[assembly: ProvideCodeBase(AssemblyName = "Clide.Extensibility")]
[assembly: ProvideCodeBase(AssemblyName = "Clide.Interfaces")]

namespace Clide
{
    /// <summary>
    /// Package providing Clide registration.
    /// </summary>
    [Guid("fde81948-30cf-4611-a9ff-c30ca1e399aa")]
    [PackageRegistration(RegisterUsing = RegistrationMethod.CodeBase, UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideBindingPath]
    public class ClidePackage : AsyncPackage
    {
    }
}