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
    using Microsoft.ComponentModel.Composition.Diagnostics;
    using System.IO;
    using Clide.Diagnostics;
    using Clide.Composition;
    using Clide.Commands;

    internal class Host<TPackage, TExport> : IHost<TPackage, TExport>
        where TPackage : Package, TExport
    {
        private static readonly ITracer tracer;
        private static readonly Guid OutputPaneId = new Guid("{9F85C038-88BE-4DE3-B2FB-D49BDFC33E8D}");

        private CompositionContainer container;
        private IServiceProvider serviceProvider;
        private Lazy<TExport> loadedPackage;
        private TraceOutputWindowManager outputWindowManager;
        private bool initializePackage = true;

        [Import]
        public ICommandManager Commands { get; set; }

        [Import]
        public IOptionsManager Options { get; set; }

        public ICompositionService Composition { get { return this.container; } }

        static Host()
        {
            Tracer.Initialize(new TracerManager());

            var tracerName = "Clide.Hosting." + Tracer.NameFor<TPackage>();

            tracer = Tracer.Get(tracerName);
            Tracer.Manager.SetTracingLevel(tracerName, SourceLevels.Information);

#if DEBUG
            Tracer.Manager.SetTracingLevel(TracerManager.DefaultSourceName, SourceLevels.All);
#else
            Tracer.Manager.SetTracingLevel(TracerManager.DefaultSourceName, SourceLevels.Warning);
#endif

            if (Debugger.IsAttached)
                Tracer.Manager.SetTracingLevel(TracerManager.DefaultSourceName, SourceLevels.All);
        }

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
                    throw new InvalidOperationException(Strings.Host.MissingGuidAttribute(this.GetType()));

                var guid = new Guid(guidString);
                var vsPackage = default(IVsPackage);

                var vsShell = ServiceProvider.GlobalProvider.GetService<SVsShell, IVsShell>();
                vsShell.IsPackageLoaded(ref guid, out vsPackage);

                if (vsPackage == null)
                    ErrorHandler.ThrowOnFailure(vsShell.LoadPackage(ref guid, out vsPackage));

                return (TPackage)vsPackage;
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
        /// Initializes the host for the given package.
        /// </summary>
        public void Initialize(TPackage package)
        {
            try
            {
                using (tracer.StartActivity("Initializing package"))
                {
                    initializePackage = false;
                    // Causes the lazy export to be initialized.
                    tracer.Info("Initializing package {0}", this.loadedPackage.Value);

                    // The instance of both the export and the currently initializing package 
                    // is the same.
                    Debug.Assert(object.ReferenceEquals(this.HostingPackage.Value, package), "Package instance is not the same");

                    // Brings in IDevEnv
                    this.Composition.SatisfyImportsOnce(this);

                    tracer.Info("Registering package commands");
                    this.Commands.AddCommands(package);
                    tracer.Info("Registering package command filters");
                    this.Commands.AddFilters(package);
                    tracer.Info("Registering package options pages");
                    this.Options.AddPages(package);

                    // Brings in imports that the package itself might need.
                    this.Composition.SatisfyImportsOnce(package);

                    tracer.Info("Package initialization finished successfully");
                }
            }
            catch (Exception ex)
            {
                tracer.Error(ex, Strings.Host.FailedToInitialize);
                throw;
            }
        }

        /// <summary>
        /// Initializes the composition service for Clide.
        /// </summary>
        private void InitializeContainer(string catalogName)
        {
            using (tracer.StartActivity("Initializing composition container"))
            {
                var composition = this.serviceProvider.GetService<SComponentModel, IComponentModel>();
                tracer.Info("Retrieving composition catalog '{0}'.", catalogName);
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
                        // We always expose our own composition service and export provider.
                        SingletonCatalog.Create<ICompositionService>(ContractNames.ICompositionService, new Lazy<ICompositionService>(() => this.Composition)),
                        SingletonCatalog.Create<ExportProvider>(ContractNames.ExportProvider, new Lazy<ExportProvider>(() => this.container)),
                        vsCatalog);
                }
                else
                {
                    tracer.Warn("Clide should not be added as a MEF component in the package manifest!");

                    vsCatalog = new AggregateCatalog(
                        // We always provide the export for the hosting package itself. 
                        // This allows components to cause the package to load automatically 
                        // whenver they are used.
                        SingletonCatalog.Create<TExport>(this.HostingPackage),
                        // We always expose our own composition service and export provider.
                        SingletonCatalog.Create<ICompositionService>(ContractNames.ICompositionService, new Lazy<ICompositionService>(() => this.Composition)),
                        SingletonCatalog.Create<ExportProvider>(ContractNames.ExportProvider, new Lazy<ExportProvider>(() => this.container)),
                        vsCatalog);
                }

                this.container = new CompositionContainer(vsCatalog, composition.DefaultExportProvider);
                var vsContainer = VsCompositionContainer.Create(new LocalOnlyExportProvider(this.container));//, HostFactory.CreateExportSettings());
                //var vsContainer = VsCompositionContainer.Create(this.container, HostFactory.CreateExportSettings());

                tracer.Info("Composition container created successfully");

                this.outputWindowManager = new TraceOutputWindowManager(
                    this.serviceProvider,
                    this.container.GetExportedValue<IShellEvents>(),
                    Tracer.Manager,
                    OutputPaneId,
                    Strings.OutputPaneTitle);

                var info = new CompositionInfo(vsCatalog, container);
                var rejected = info.PartDefinitions.Where(part => part.IsPrimaryRejection).ToList();
                if (rejected.Count > 0)
                {
                    tracer.Error("Found {0} rejected parts in composition container!", rejected.Count);
                    var writer = new StringWriter();
                    rejected.ForEach(part => PartDefinitionInfoTextFormatter.Write(part, writer));
                    tracer.Error(writer.ToString());
                }
                else
                {
                    tracer.Info("No parts were rejected in the composition container");
                }

#if DEBUG
                // Log information about the composition container in debug mode.
                {
                    var infoWriter = new StringWriter();
                    CompositionInfoTextFormatter.Write(info, infoWriter);
                    tracer.Info(infoWriter.ToString());
                }
#else
            if (Debugger.IsAttached)
            {
                // Log information about the composition container when debugger is attached too.
                var infoWriter = new StringWriter();
                CompositionInfoTextFormatter.Write(info, infoWriter);
                tracer.Info(infoWriter.ToString());
            }
#endif

                this.container.ComposeExportedValue(this.HostingPackage);
                tracer.Info("Composition container initialization finished");
            }
        }

        /// <summary>
        /// Provides lazy access to the loaded hosting package, and exports it 
        /// automatically for consumption.
        /// </summary>
        [Export]
        public Lazy<TExport> HostingPackage { get; private set; }
    }
}
