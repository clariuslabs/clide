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
    using System;
    using System.Diagnostics;
    using System.Dynamic;
    using System.Linq;
    using Clide.Properties;
    using Microsoft.VisualStudio.ComponentModelHost;
    using System.ComponentModel.Composition.Hosting;
    using System.ComponentModel.Composition;
    using Clide.Composition;
    using Microsoft.ComponentModel.Composition.Diagnostics;
    using System.IO;
    using System.Collections.Generic;
    using System.ComponentModel.Composition.Primitives;
    using System.Collections.Concurrent;
    using System.Xml.Linq;

    internal class DevEnvFactory
    {
        private static readonly string ClideAssembly = Path.GetFileName(typeof(IDevEnv).Assembly.ManifestModule.FullyQualifiedName);
        private static readonly ITracer tracer = Tracer.Get<DevEnvFactory>();
        private ConcurrentDictionary<IServiceProvider, CompositionContainer> serviceContainers = new ConcurrentDictionary<IServiceProvider, CompositionContainer>();

        public IDevEnv Get(IServiceProvider services)
        {
            return serviceContainers
                .GetOrAdd(services, s => InitializeContainer(s))
                .GetExportedValue<IDevEnv>();
        }

        private CompositionContainer InitializeContainer(IServiceProvider services)
        {
            using (tracer.StartActivity(Strings.DevEnvFactory.CreatingComposition))
            {
                var container = default(CompositionContainer);
                var catalogs = new List<ComposablePartCatalog>
                {
                    new LocalDecoratingCatalog(new AssemblyCatalog(typeof(IDevEnv).Assembly)),
                    SingletonCatalog.Create<ICompositionService>(ContractNames.ICompositionService, new Lazy<ICompositionService>(() => container)),
                    SingletonCatalog.Create<CompositionContainer>(ContractNames.CompositionContainer, new Lazy<CompositionContainer>(() => container)),
                    SingletonCatalog.Create<ExportProvider>(ContractNames.ExportProvider, new Lazy<ExportProvider>(() => container)),
                };

                var installPath = GetInstallPath(services);
                var packageManifestFile = Path.Combine(installPath, "extension.vsixmanifest");
                if (File.Exists(packageManifestFile))
                {
                    var manifestDoc = XDocument.Load(packageManifestFile);

                    ThrowIfClideIsMefComponent(manifestDoc);
                    WarnIfClideComponentIsAlsoMefComponent(packageManifestFile, manifestDoc);

                    foreach (string clideComponent in GetClideComponents(manifestDoc))
                    {
                        if (clideComponent == ClideAssembly)
                        {
                            tracer.Warn(Strings.DevEnvFactory.ClideNotNecessaryAsComponent(clideComponent));
                            continue;
                        }

                        var assemblyFile = Path.Combine(installPath, clideComponent);
                        if (!File.Exists(assemblyFile))
                            throw new InvalidOperationException(Strings.DevEnvFactory.ClideComponentNotFound(packageManifestFile, clideComponent, assemblyFile));

                        catalogs.Add(new LocalDecoratingCatalog(new AssemblyCatalog(assemblyFile)));
                    }
                }

                var composition = services.GetService<SComponentModel, IComponentModel>();

                var catalog = new AggregateCatalog(catalogs);
                container = new CompositionContainer(catalog, composition.DefaultExportProvider);
                
                AppDomain.CurrentDomain.GetAssemblies()
                    .First(asm => asm.FullName.StartsWith("Microsoft.VisualStudio.ExtensibilityHosting"))
                    .GetType("Microsoft.VisualStudio.ExtensibilityHosting.VsCompositionContainer")
                    .AsDynamicReflection()
                    .Create(new LocalOnlyExportProvider(container));

                Log(container, catalog);

                return container;
            }
        }

        private static void Log(CompositionContainer container, ComposablePartCatalog catalog)
        {
            var info = new CompositionInfo(catalog, container);
            var rejected = info.PartDefinitions.Where(part => part.IsPrimaryRejection).ToList();
            if (rejected.Count > 0)
            {
                tracer.Error(Strings.DevEnvFactory.CompositionErrors(rejected.Count));
                var writer = new StringWriter();
                rejected.ForEach(part => PartDefinitionInfoTextFormatter.Write(part, writer));
                tracer.Error(writer.ToString());
                throw new InvalidOperationException(
                    Strings.DevEnvFactory.CompositionErrors(rejected.Count) + Environment.NewLine +
                    writer.ToString());
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
    }
}
