namespace IntegrationPackage
{
    using Clide;
    using Clide.Diagnostics;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;

    [Guid(Constants.PackageGuid)]
    [ProvideAutoLoad(UIContextGuids.NoSolution)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0")]
    [DisplayName("Clide Test Package")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(MyToolWindow))]
    public class ClideIntegrationPackage : Package
    {
        private ITracer tracer;

        static ClideIntegrationPackage()
        {
            LocalResolver.Initialize(Path.GetDirectoryName(typeof(ClideIntegrationPackage).Assembly.Location));
        }

        protected override void Initialize()
        {
            base.Initialize();

            var devEnv = Host.Initialize(this);

            Tracer.Manager.AddListener(this.GetType().Namespace, new TextTraceListener(devEnv.OutputWindow.GetPane(this)));
            Tracer.Manager.SetTracingLevel(this.GetType().Namespace, SourceLevels.All);

            this.tracer = Tracer.Get<ClideIntegrationPackage>();

            this.tracer.Info("Shell package initialized");
        }
    }
}
