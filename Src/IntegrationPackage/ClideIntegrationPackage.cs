namespace IntegrationPackage
{
    using System;
    using System.ComponentModel.Design;
    using System.Runtime.InteropServices;
    using System.Linq;
    using Microsoft.VisualStudio.ComponentModelHost;
    using Microsoft.VisualStudio.Shell;
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Shell.Interop;
    using System.ComponentModel;
    using System.Collections.Generic;
    using System.ComponentModel.Composition.Hosting;
    using Clide;
    using System.Diagnostics;
    using System.IO;
    using Clide.Diagnostics;

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
