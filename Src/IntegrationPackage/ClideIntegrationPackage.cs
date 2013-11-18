namespace IntegrationPackage
{
    using Clide;
    using Clide.Diagnostics;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    [Guid(Constants.PackageGuid)]
    [ProvideAutoLoad(UIContextGuids.NoSolution)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(MyToolWindow))]
    public class ClideIntegrationPackage : Package
    {
        private static readonly string OutputPaneTitle = "Clide Integration Test Package";

        private ITracer tracer;
        private IDisposable host;

        static ClideIntegrationPackage()
        {
            LocalResolver.Initialize(Path.GetDirectoryName(typeof(ClideIntegrationPackage).Assembly.Location));
        }

        protected override void Initialize()
        {
            base.Initialize();

            this.host = Host.Initialize(this, OutputPaneTitle, "*");
            this.tracer = Tracer.Get<ClideIntegrationPackage>();

            this.tracer.Info("Shell package initialized");
            this.tracer.Info("Composition log path: {0}", Path.Combine(Path.GetTempPath(), "DevEnv.log"));
        }
    }
}
