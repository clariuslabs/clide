using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.ExtensibilityHosting;
using System.ComponentModel.Composition;
using Clide;

[assembly:VsCatalogName("Package1")]

namespace Clarius.ClidePackage1
{
    [ProvideAutoLoad(UIContextGuids.SolutionExists)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(GuidList.guidClidePackage1PkgString)]
    public sealed class ClidePackage1Package : Package
    {
        private Clide.IHost<ClidePackage1Package, ClidePackage1Package> host;

        public ClidePackage1Package()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
            this.host = Clide.HostFactory.CreateHost<ClidePackage1Package, ClidePackage1Package>(ServiceProvider.GlobalProvider, "Package1");
        }

        protected override void Initialize()
        {
            Trace.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();
            this.host.Initialize(this);
            this.Messages.ShowInformation("Package 1 Clide version: " + host.GetType().Assembly.GetName().Version);
        }

        [Import]
        public IMessageBoxService Messages { get; set; }
    }
}
