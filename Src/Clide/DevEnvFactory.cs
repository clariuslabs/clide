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
    using System.IO;
    using System.Collections.Generic;
    using System.ComponentModel.Composition.Primitives;
    using System.Collections.Concurrent;
    using System.Xml.Linq;
    using System.Threading.Tasks;
    using Clide.Diagnostics;
    using Microsoft.Practices.ServiceLocation;
    using Autofac;
    using Autofac.Extras.CommonServiceLocator;
    using System.Reflection;
    using Clide.Composition;
    using Autofac.Extras.Attributed;

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
                var builder = new ContainerBuilder();
                // Allow dependencies of VS exported services.
                var composition = services.GetService<SComponentModel, IComponentModel>();
                builder.RegisterComponentModel(composition);
                // Allow dependencies of non-exported VS services.
                builder.RegisterServiceProvider(services);
                // Automatically registers metadata associated via metadata attributes on components.
                builder.RegisterModule(new AttributedMetadataModule());
                builder.Register<IServiceLocator>(c => serviceLocators[services]);

                // Keep track of assemblies we've already added, to avoid duplicate registrations.
                var addedAssemblies = new HashSet<string>();

                // Register built-in components from Clide assembly.
                RegisterAssembly(builder, composition, Assembly.GetExecutingAssembly(), addedAssemblies);

                // Register hosting package assembly.
                RegisterAssembly(builder, composition, services.GetType().Assembly, addedAssemblies);

                var installPath = GetInstallPath(services);

                var packageManifestFile = Path.Combine(installPath, "extension.vsixmanifest");
                if (File.Exists(packageManifestFile))
                {
                    tracer.Info(Strings.DevEnvFactory.ExtensionManifestFound(packageManifestFile));
                    var manifestDoc = XDocument.Load(packageManifestFile);

                    ThrowIfClideIsMefComponent(manifestDoc);
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

                        RegisterAssembly(builder, composition, Assembly.LoadFrom(assemblyFile), addedAssemblies);
                    }
                }
                else
                {
                    tracer.Info(Strings.DevEnvFactory.ExtensionManifestNotFound(packageManifestFile));
                }

                var container = builder.Build();

                return new AutofacServiceLocator(container);
            }
        }

        private static void RegisterAssembly(ContainerBuilder builder, IComponentModel composition, Assembly assembly, HashSet<string> addedAssemblies)
        {
            var assemblyFile = assembly.Location.ToLowerInvariant();
            if (!addedAssemblies.Contains(assemblyFile))
            {
                builder.RegisterAssemblyComponents(assembly)
                    .WithImports(composition.DefaultExportProvider)
                    .WithKeyFilter()
                    .WithMetadataFilter();
                
                addedAssemblies.Add(assemblyFile);
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
    }
}
