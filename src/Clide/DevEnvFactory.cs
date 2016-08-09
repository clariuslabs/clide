#region BSD License
/* 
Copyright (c) 2012, Clarius Consulting
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

namespace Clide
{
	using Clide.CommonComposition;
	using Clide.Composition;
	using Clide.Diagnostics;
	using Clide.Properties;
	using Microsoft.Practices.ServiceLocation;
	using Microsoft.VisualStudio.ComponentModelHost;
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.ComponentModel.Composition;
	using System.ComponentModel.Composition.Hosting;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Xml.Linq;
	using System.Diagnostics;

	internal class DevEnvFactory
    {
        private static readonly string ClideAssembly = Path.GetFileName(typeof(IDevEnv).Assembly.ManifestModule.FullyQualifiedName);
        private static readonly ITracer tracer = Tracer.Get<DevEnvFactory>();
        private ConcurrentDictionary<IServiceProvider, IServiceLocator> serviceLocators = new ConcurrentDictionary<IServiceProvider, IServiceLocator>();

        public IDevEnv Get(IServiceProvider services)
        {
            return serviceLocators
                .GetOrAdd(services, s => InitializeContainer(s))
                .GetInstance<IDevEnv>();
        }

        private IServiceLocator InitializeContainer(IServiceProvider services)
        {
            using (tracer.StartActivity(Strings.DevEnvFactory.CreatingComposition))
            {
                // Allow dependencies of VS exported services.
                var composition = services.TryGetService<SComponentModel, IComponentModel>();

                // Keep track of assemblies we've already added, to avoid duplicate registrations.
                var addedAssemblies = new Dictionary<string, Assembly>();

                // Register built-in components from Clide assembly.
                var clideAssembly = Assembly.GetExecutingAssembly();
                addedAssemblies.Add(clideAssembly.Location.ToLowerInvariant(), clideAssembly);

                // Register hosting package assembly.
                var servicesAssembly = services.GetType().Assembly;
                addedAssemblies[servicesAssembly.Location.ToLowerInvariant()] = servicesAssembly;

                var installPath = GetInstallPath(services);

                foreach (var providedAssemblyFile in services.GetType()
                    .GetCustomAttributes<ProvideComponentsAttribute>(true)
                    .Select(attr => Path.Combine(installPath, attr.AssemblyFile)))
                {
                    if (!File.Exists(providedAssemblyFile))
                        throw new InvalidOperationException(Strings.DevEnvFactory.ClideProvidedComponentsNotFound(
                            services.GetType().FullName, Path.GetFileName(providedAssemblyFile), providedAssemblyFile));

                    var providedAssembly = Assembly.LoadFrom(providedAssemblyFile);
                    if (!addedAssemblies.ContainsKey(providedAssembly.Location.ToLowerInvariant()))
                        addedAssemblies.Add(providedAssembly.Location.ToLowerInvariant(), providedAssembly);
                }

                var packageManifestFile = Path.Combine(installPath, "extension.vsixmanifest");
                if (File.Exists(packageManifestFile))
                {
                    tracer.Info(Strings.DevEnvFactory.ExtensionManifestFound(packageManifestFile));
                    var manifestDoc = XDocument.Load(packageManifestFile);

                    ThrowIfClideIsMefComponent(manifestDoc);
                    // NOTE: we don't warn anymore in this case, since a single package may have 
                    // a mix of plain VS exports as well as clide components. 
                    // Since we use CommonComposition, only the types with the ComponentAttribute 
                    // will be made available in the Clide container, not the others, and no 
                    // duplicates would be registered.
                    //WarnIfClideComponentIsAlsoMefComponent(packageManifestFile, manifestDoc);

                    foreach (string clideComponent in GetClideComponents(manifestDoc))
                    {
                        var assemblyFile = Path.Combine(installPath, clideComponent);
                        tracer.Info(Strings.DevEnvFactory.ClideComponentDeclared(clideComponent, assemblyFile));

                        if (clideComponent == ClideAssembly)
                        {
                            tracer.Warn(Strings.DevEnvFactory.ClideNotNecessaryAsComponent(clideComponent));
                            continue;
                        }

                        if (!File.Exists(assemblyFile))
                            throw new InvalidOperationException(Strings.DevEnvFactory.ClideComponentNotFound(packageManifestFile, clideComponent, assemblyFile));

                        var componentAssembly = Assembly.LoadFrom(assemblyFile);
                        if (!addedAssemblies.ContainsKey(componentAssembly.Location.ToLowerInvariant()))
                            addedAssemblies.Add(componentAssembly.Location.ToLowerInvariant(), componentAssembly);
                    }
                }
                else
                {
                    tracer.Info(Strings.DevEnvFactory.ExtensionManifestNotFound(packageManifestFile));
                }

                var catalog = new ComponentCatalog(addedAssemblies.Values.ToArray());
				var providers = composition != null ? 
					new ExportProvider[] { composition.DefaultExportProvider } : 
					new ExportProvider[0];

                var container = new CompositionContainer(catalog, providers);

                // Make the service locator itself available as an export.
                var serviceLocator = new ServicesAccessor(services, new Lazy<IServiceLocator>(() => serviceLocators[services]));
                container.ComposeParts(serviceLocator);

                return new FallbackServiceLocator(new ExportsServiceLocator(container), new ServiceProviderLocator(services));
            }
        }

        private void ThrowIfClideIsMefComponent(XDocument doc)
        {
            var isClideMef2010 = doc
                .Descendants("{http://schemas.microsoft.com/developer/vsx-schema/2010}MefComponent")
                .Any(element => element.Value == ClideAssembly);
            var isClideMef2012 = doc
                .Descendants("{http://schemas.microsoft.com/developer/vsx-schema/2011}Asset")
                .Any(element =>
                    element.Attribute("Type").Value == "Microsoft.VisualStudio.MefComponent" &&
                    element.Attribute("Path").Value == ClideAssembly);

            if (isClideMef2010 || isClideMef2012)
                throw new InvalidOperationException(Strings.DevEnvFactory.ClideCantBeMefComponent(ClideAssembly));
        }

        private void WarnIfClideComponentIsAlsoMefComponent(string packageManifestFile, XDocument doc)
        {
            var clideComponents = GetClideComponents(doc);
            var mefComponents = new HashSet<string>(doc
                .Descendants("{http://schemas.microsoft.com/developer/vsx-schema/2010}MefComponent")
                .Select(element => element.Value)
                .Concat(doc
                    .Descendants("{http://schemas.microsoft.com/developer/vsx-schema/2011}Asset")
                    .Where(element => element.Attribute("Type").Value == "Microsoft.VisualStudio.MefComponent")
                    .Select(element => element.Attribute("Path").Value)));

            var duplicates = clideComponents.Where(clide => mefComponents.Contains(clide)).ToList();
            if (duplicates.Count != 0)
                tracer.Warn(Strings.DevEnvFactory.ClideComponentAlsoMefComponent(packageManifestFile,
                    string.Join(", ", duplicates)));
        }

        private IEnumerable<string> GetClideComponents(XDocument doc)
        {
            return doc
                .Descendants("{http://schemas.microsoft.com/developer/vsx-schema/2010}CustomExtension")
                .Where(element => element.Attribute("Type").Value == "ClideComponent")
                .Select(element => element.Value)
                .Concat(doc
                    .Descendants("{http://schemas.microsoft.com/developer/vsx-schema/2011}Asset")
                    .Where(element => element.Attribute("Type").Value == "ClideComponent")
                    .Select(element => element.Attribute("Path").Value));
        }

        private string GetInstallPath(IServiceProvider services)
        {
            return Path.GetDirectoryName(services.GetType().Assembly.ManifestModule.FullyQualifiedName);
        }

        internal class ServicesAccessor
        {
            private Lazy<IServiceLocator> serviceLocator;

            public ServicesAccessor(IServiceProvider serviceProvider, Lazy<IServiceLocator> serviceLocator)
            {
                this.ServiceProvider = serviceProvider;
                this.serviceLocator = serviceLocator;
            }

            [Export]
            public IServiceProvider ServiceProvider { get; private set; }

            [Export]
            public IServiceLocator ServiceLocator { get { return serviceLocator.Value; } }
        }
    }
}
