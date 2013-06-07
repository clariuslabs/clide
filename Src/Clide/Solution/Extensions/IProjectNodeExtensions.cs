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

namespace Clide.Solution
{
    using System.Collections.Generic;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.Shell.Design;
    using System.Reflection;
    using System.ComponentModel.Design;
    using System.IO;
    using Clide.Properties;
    using System.Diagnostics;
    using VSLangProj;
    using Clide.Diagnostics;
    using Clide.VisualStudio;
    using System.Threading;

    /// <summary>
    /// Provides usability extensions to the <see cref="IProjectNode"/>.
    /// </summary>
    public static class IProjectNodeExtensions
    {
        private static readonly ITracer tracer = Tracer.Get(typeof(IProjectNodeExtensions));

        public static void Build(this IProjectNode project)
        {
            var solution = project.OwningSolution;
            if (solution == null)
                throw new ArgumentException(Strings.IProjectNodeExtensions.BuildNoSolution(project.DisplayName));

            var dte = solution.As<EnvDTE.Solution>();
            if (dte == null)
                throw new ArgumentException(Strings.IProjectNodeExtensions.BuildNoSolution(project.DisplayName));

            var build = (EnvDTE80.SolutionBuild2)dte.SolutionBuild;
            
            dte.SolutionBuild.BuildProject(build.ActiveConfiguration.Name, project.As<EnvDTE.Project>().UniqueName, true);
        }

        public static Assembly GetOutputAssembly(this IProjectNode project)
        {
            var fileName = (string)project.Properties.TargetFileName;
            var outDir = (string)Path.Combine(
                project.Properties.MSBuildProjectDirectory,
                project.Properties.BaseIntermediateOutputPath,
                project.Configuration.ActiveConfiguration);

            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(outDir))
            {
                tracer.Warn(Strings.IProjectNodeExtensions.NoTargetAssemblyName(project.DisplayName));
                return null;
            }

            var assemblyFile = Path.Combine(outDir, fileName);

            if (!File.Exists(assemblyFile))
            {
                project.Build();
                for (int i = 0; i < 5; i++)
                {
                    if (File.Exists(assemblyFile))
                        break;

                    Thread.Sleep(200);
                }

                if (!File.Exists(assemblyFile))
                {
                    tracer.Warn(Strings.IProjectNodeExtensions.NoBuildOutput(project.DisplayName, assemblyFile));
                    return null;
                }
            }

            var assemblyName = AssemblyName.GetAssemblyName(assemblyFile);
            var vsProject = project.As<IVsHierarchy>();
            var localServices = project.As<IServiceProvider>();
            var globalServices = GlobalServiceProvider.Instance;

            if (vsProject == null ||
                localServices == null ||
                globalServices == null)
            {
                tracer.Warn(Strings.IProjectNodeExtensions.InvalidVsContext);
                return null;
            }

            var openScope = globalServices.GetService<SVsSmartOpenScope, IVsSmartOpenScope>();
            var dtar = localServices.GetService<SVsDesignTimeAssemblyResolution, IVsDesignTimeAssemblyResolution>();

            // As suggested by Christy Henriksson, we reuse the type discovery service 
            // but just for the IDesignTimeAssemblyLoader interface. The actual 
            // assembly reading is done by the TFP using metadata only :)
            var dts = globalServices.GetService<DynamicTypeService>();
            var ds = dts.GetTypeDiscoveryService(vsProject);
            var dtal = ds as IDesignTimeAssemblyLoader;

            if (openScope == null || dtar == null || dts == null || ds == null || dtal == null)
            {
                tracer.Warn(Strings.IProjectNodeExtensions.InvalidTypeContext);
                return null;
            }

            var provider = new VsTargetFrameworkProvider(dtar, dtal, openScope);

            return provider.GetReflectionAssembly(assemblyName);
        }

        public static IEnumerable<Assembly> GetReferencedAssemblies(this IProjectNode project)
        {
            var vsProject = project.As<IVsHierarchy>();
            var vsLangProject = project.As<VSProject>();
            var localServices = project.As<IServiceProvider>();
            var globalServices = GlobalServiceProvider.Instance;

            if (vsProject == null ||
                localServices == null ||
                globalServices == null)
            {
                tracer.Warn(Strings.IProjectNodeExtensions.InvalidVsContext);
                return Enumerable.Empty<Assembly>();
            }

            var openScope = globalServices.GetService<SVsSmartOpenScope, IVsSmartOpenScope>();
            var dtar = localServices.GetService<SVsDesignTimeAssemblyResolution, IVsDesignTimeAssemblyResolution>();

            // As suggested by Christy Henriksson, we reuse the type discovery service 
            // but just for the IDesignTimeAssemblyLoader interface. The actual 
            // assembly reading is done by the TFP using metadata only :)
            var dts = globalServices.GetService<DynamicTypeService>();
            var ds = dts.GetTypeDiscoveryService(vsProject);
            var dtal = ds as IDesignTimeAssemblyLoader;

            if (openScope == null || dtar == null || dts == null || ds == null || dtal == null)
            {
                tracer.Warn(Strings.IProjectNodeExtensions.InvalidTypeContext);
                return Enumerable.Empty<Assembly>();
            }

            var provider = new VsTargetFrameworkProvider(dtar, dtal, openScope);

            return vsLangProject.References
                .OfType<Reference>()
                .Select(x => TryLoad(provider, x))
                .Where(x => x != null);
        }

        private static Assembly TryLoad(VsTargetFrameworkProvider provider, Reference reference)
        {
            try
            {
                var name = AssemblyName.GetAssemblyName(reference.Path);
                return provider.GetReflectionAssembly(name);
            }
            catch (Exception e)
            {
                tracer.Warn(e, Strings.IProjectNodeExtensions.FailedToLoadAssembly(reference.Name, reference.Path));
                return null;
            }
        }
    }
}
