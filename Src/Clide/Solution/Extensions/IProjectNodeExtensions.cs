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
    using Clide.Diagnostics;
    using Clide.Properties;
    using Microsoft.VisualStudio.Shell.Design;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using VSLangProj;

    /// <summary>
    /// Provides usability extensions to the <see cref="IProjectNode"/> interface.
    /// </summary>
    public static class IProjectNodeExtensions
    {
        private static readonly ITracer tracer = Tracer.Get(typeof(IProjectNodeExtensions));

        /// <summary>
        /// Builds the specified project.
        /// </summary>
        /// <param name="project">The project to build.</param>
        /// <exception cref="System.ArgumentException">The project has no <see cref="ISolutionExplorerNode.OwningSolution"/>.</exception>
        /// <returns><see langword="true"/> if the build succeeded; <see langword="false"/> otherwise.</returns>
        public static Task<bool> Build(this IProjectNode project)
        {
            var solution = project.OwningSolution;
            if (solution == null)
                throw new ArgumentException(Strings.IProjectNodeExtensions.BuildNoSolution(project.DisplayName));

            var sln = solution.As<EnvDTE.Solution>();
            if (sln == null)
                throw new ArgumentException(Strings.IProjectNodeExtensions.BuildNoSolution(project.DisplayName));

            return System.Threading.Tasks.Task.Factory.StartNew<bool>(() => 
            {
                var mre = new ManualResetEventSlim();
                var events = sln.DTE.Events.BuildEvents;
                EnvDTE._dispBuildEvents_OnBuildDoneEventHandler done = (scope, action) => mre.Set();
                events.OnBuildDone += done;
                try
                {
                    // Let build run async.
                    sln.SolutionBuild.BuildProject(sln.SolutionBuild.ActiveConfiguration.Name, project.As<EnvDTE.Project>().UniqueName, false);

                    // Wait until it's done.
                    mre.Wait();

                    // LastBuildInfo == # of projects that failed to build.
                    return sln.SolutionBuild.LastBuildInfo == 0;
                }
                catch (Exception ex)
                {
                    tracer.Error(ex, Strings.IProjectNodeExtensions.BuildException);
                    return false;
                }
                finally
                {
                    // Cleanup handler.
                    events.OnBuildDone -= done;
                }
            });
        }

        /// <summary>
        /// Gets the output assembly of the given project. If the project 
        /// was never built before, it's built before returning the output 
        /// assembly.
        /// </summary>
        /// <param name="project">The project to get the output assembly from.</param>
        public static Task<Assembly> GetOutputAssembly(this IProjectNode project)
        {
            var fileName = (string)project.Properties.TargetFileName;
            var outDir = (string)Path.Combine(
                project.Properties.MSBuildProjectDirectory,
                // NOTE: we load from the obj/Debug|Release folder, which is 
                // the one built in the background by VS continuously.
                project.Properties.BaseIntermediateOutputPath,
                project.Configuration.ActiveConfiguration);

            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(outDir))
            {
                tracer.Warn(Strings.IProjectNodeExtensions.NoTargetAssemblyName(project.DisplayName));
                return TaskHelpers.FromResult<Assembly>(null);
            }

            var assemblyFile = Path.Combine(outDir, fileName);

            return Task.Factory.StartNew<Assembly>(() =>
            {
                if (!File.Exists(assemblyFile))
                {
                    var success = project.Build().Result;
                    if (success)
                    {
                        // Let the build finish writing the file
                        for (int i = 0; i < 5; i++)
                        {
                            if (File.Exists(assemblyFile))
                                break;

                            Thread.Sleep(200);
                        }
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
            });
        }

        /// <summary>
        /// Gets the referenced assemblies from the given project.
        /// </summary>
        /// <param name="project">The project containing references.</param>
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
