#region BSD License
/* 
Copyright (c) 2012, Clarius Consulting
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion
namespace Clide
{
    using Clide.Properties;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.ComponentModelHost;
    using Microsoft.VisualStudio.ExtensibilityHosting;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using System.Linq;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    internal class Host<TPackage, TExport> : IHost<TPackage, TExport>
        where TPackage : Package, TExport
    {
        private static readonly ITracer tracer = Tracer.Get<TPackage>();

        private CompositionContainer container;
        private IServiceProvider serviceProvider;
        private Lazy<TExport> loadedPackage;
        private bool initializePackage = true;

        [Import]
        public IDevEnv DevEnv { get; set; }
        public ICompositionService Composition { get { return this.container; } }

        public Host(IServiceProvider serviceProvider, string catalogName)
        {
            this.serviceProvider = serviceProvider;

            this.loadedPackage = new Lazy<TExport>(() =>
            {
                var guidString = typeof(TPackage)
                    .GetCustomAttributes(true)
                    .OfType<GuidAttribute>()
                    .Select(g => g.Value)
                    .FirstOrDefault();

                if (guidString == null)
                    throw new InvalidOperationException(Strings.HostingPackage.MissingGuidAttribute(this.GetType()));

                var guid = new Guid(guidString);
                var vsPackage = default(IVsPackage);

                var vsShell = ServiceProvider.GlobalProvider.GetService<SVsShell, IVsShell>();
                vsShell.IsPackageLoaded(ref guid, out vsPackage);

                if (vsPackage == null)
                    ErrorHandler.ThrowOnFailure(vsShell.LoadPackage(ref guid, out vsPackage));

                var package = vsPackage as TPackage;
                if (package == null)
                    throw new InvalidOperationException(Strings.HostingPackage.PackageBaseRequired(typeof(TPackage)));

                return package;
            });

            this.HostingPackage = new Lazy<TExport>(() =>
            {
                if (initializePackage)
                    Initialize((TPackage)this.loadedPackage.Value);

                return this.loadedPackage.Value;
            });

            InitializeContainer(catalogName);
        }

        /// <summary>
        /// Forcedly initializes the package.
        /// </summary>
        public void Initialize(TPackage package)
        {
            initializePackage = false;
            // Causes the lazy export to be initialized.
            tracer.Info("Initialized package {0}", this.HostingPackage.Value);

            // The instance of both the export and the currently initializing package 
            // is the same.
            Debug.Assert(object.ReferenceEquals(this.HostingPackage.Value, package), "Package instance is not the same");

            // Brings in IDevEnv
            this.Composition.SatisfyImportsOnce(this);

            this.DevEnv.Commands.AddCommands(package);
            this.DevEnv.Commands.AddFilters(package);

            // Brings in imports that the package itself might need.
            this.Composition.SatisfyImportsOnce(package);
        }

        /// <summary>
        /// Initializes the composition service for Clide.
        /// </summary>
        private void InitializeContainer(string catalogName)
        {
            var composition = this.serviceProvider.GetService<SComponentModel, IComponentModel>();
            var vsCatalog = composition.GetCatalog(catalogName);

            // Determine if the user added or not Clide as a MefComponent in the VSIX
            var isClideInVsCatalog = vsCatalog.Parts.Any(part =>
                part.ExportDefinitions.Any(export => export.ContractName == typeof(IDevEnv).FullName));

            // It would be user mistake to add Clide to MEF, but we have to account for it 
            // anyway.
            if (!isClideInVsCatalog)
            {
                vsCatalog = new AggregateCatalog(
                    // Clide is not on the manifest as MefComponent, therefore, it's not on the catalog.
                    new AssemblyCatalog(typeof(IDevEnv).Assembly),
                    // We always provide the export for the hosting package itself. 
                    // This allows components to cause the package to load automatically 
                    // whenver they are used.
                    SingletonCatalog.Create<TExport>(this.HostingPackage),
                    vsCatalog);
            }
            else
            {
                vsCatalog = new AggregateCatalog(
                    // We always provide the export for the hosting package itself. 
                    // This allows components to cause the package to load automatically 
                    // whenver they are used.
                    SingletonCatalog.Create<TExport>(this.HostingPackage),
                    vsCatalog);
            }

            this.container = new CompositionContainer(vsCatalog, composition.DefaultExportProvider);
            VsCompositionContainer.Create(new LocalOnlyExportProvider(this.container));

            //this.container.ComposeExportedValue(this);
            this.container.ComposeExportedValue(this.HostingPackage);

            var e = this.container.GetExport<TExport>();
            Debug.Assert(e != null, "No package export!");
        }

        /// <summary>
        /// Initializes the components that depend on the loaded package, 
        /// like commands and filters.
        /// </summary>
        /// <param name="hostingPackage"></param>
        private void InitializePackage(TPackage hostingPackage)
        {
            this.Composition.SatisfyImportsOnce(this);

            this.DevEnv.Commands.AddCommands(hostingPackage);
            this.DevEnv.Commands.AddFilters(hostingPackage);
        }

        /// <summary>
        /// Provides lazy access to the loaded hosting package, and exports it 
        /// automatically for consumption.
        /// </summary>
        [Export]
        public Lazy<TExport> HostingPackage { get; private set; }
    }
}
