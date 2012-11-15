using Microsoft.VisualStudio.ExtensibilityHosting;
namespace Clide.Hosting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.VisualStudio.Shell;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using Microsoft.VisualStudio.ComponentModelHost;
    using System.ComponentModel.Design;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio;
    using Clide.Properties;

    /// <summary>
    /// Base class for the package that hosts Clide.
    /// </summary>
    [ComVisible(true)]
    public abstract class HostingPackage : Package, IHostingPackage
	{
        [Import]
        protected IDevEnv DevEnv { get; private set; }

		protected HostingPackage()
		{
			this.LoadedPackage = new Lazy<IHostingPackage>(() =>
			{
				var guidString = this.GetType()
					.GetCustomAttributes(true)
					.OfType<GuidAttribute>()
					.Select(g => g.Value)
					.FirstOrDefault();

				if (guidString == null)
					throw new InvalidOperationException(Strings.HostingPackage.MissingGuidAttribute(this.GetType()));

				var guid = new Guid(guidString);
				var package = default(IVsPackage);

				var vsShell = ServiceProvider.GlobalProvider.GetService<SVsShell, IVsShell>();
				vsShell.IsPackageLoaded(ref guid, out package);

				if (package == null)
					ErrorHandler.ThrowOnFailure(vsShell.LoadPackage(ref guid, out package));

				var mpf = package as HostingPackage;
				if (mpf == null)
					throw new InvalidOperationException(Strings.HostingPackage.PackageBaseRequired(typeof(HostingPackage)));

				return mpf;
			});
		}

        /// <summary>
        /// Initializes the <see cref="Composition"/> service 
        /// for Clide.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            var composition = ((IComponentModel)this.GetService(typeof(SComponentModel)));
            var vsCatalog = composition.GetCatalog(this.CatalogName);

            var catalog = new AggregateCatalog(
                new AssemblyCatalog(typeof(IDevEnv).Assembly),
                vsCatalog);

            var container = VsCompositionContainer.Create(
                catalog,
                new VsExportProviderSettings(
                    VsExportProvidingPreference.OnlyUseThisContainerIfNotInOtherContainers,
                    VsExportSharingPolicy.IncludeExportsFromOthers));

            //var container = new CompositionContainer(catalog, composition.DefaultExportProvider);
            container.ComposeExportedValue<IHostingPackage>(this);
            container.SatisfyImportsOnce(this);

            this.Composition = container;

            this.DevEnv.Commands.AddCommands(this);
            this.DevEnv.Commands.AddFilters(this);
        }

		/// <summary>
		/// Provides lazy access to the loaded hosting package.
		/// </summary>
		protected Lazy<IHostingPackage> LoadedPackage { get; private set; }

        ///// <summary>
        ///// Must export the hosting package to the global MEF container so that 
        ///// it's available to all components, even those that target built-in VS
        ///// MEF extension points. Exposed using a lazy-load mechanism to avoid forcing an 
        ///// unnecessary early load. Should export using a contract name to disambiguate 
        ///// with potentially other packages that are also leveraging Clide, 
        ///// and just expose the <see cref="LoadedPackage"/> lazy value.
        ///// </summary>
        //protected abstract IHostingPackage Package { get; }

        /// <summary>
        /// Retrieves the name of the VS catalog used by the components that 
        /// should be hosted in the Clide container.
        /// </summary>
        protected abstract string CatalogName { get; }

        /// <summary>
        /// Exposes the root composition service for this hosted instance of 
        /// Clide.
        /// </summary>
        public ICompositionService Composition { get; private set; }
    }
}
