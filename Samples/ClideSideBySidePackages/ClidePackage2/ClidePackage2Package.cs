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
using Clide;
using System.ComponentModel.Composition;

[assembly: VsCatalogName("Package2")]

namespace Clarius.ClidePackage2
{
    [ProvideAutoLoad(UIContextGuids.SolutionExists)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(GuidList.guidClidePackage2PkgString)]
    public sealed class ClidePackage2Package : Package
    {
        private Clide.IHost<ClidePackage2Package, ClidePackage2Package> host;
        
        public ClidePackage2Package()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
            this.host = Clide.HostFactory.CreateHost<ClidePackage2Package, ClidePackage2Package>(ServiceProvider.GlobalProvider, "Package1");
        }

        protected override void Initialize()
        {
            Trace.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();
            this.host.Initialize(this);

            this.Messages.ShowInformation("Package 2 Clide version: " + host.GetType().Assembly.GetName().Version);
        }

        [Import]
        public IMessageBoxService Messages { get; set; }
    }
}
