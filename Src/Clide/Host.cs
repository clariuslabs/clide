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
    using System.Linq;
    using System.Diagnostics;
    using System.ComponentModel.Composition;
    using Clide.Diagnostics;
    using Clide.Properties;
    using Clide.Commands;
    using System.Runtime.InteropServices;

    public class Host : IDisposable
    {
        private static readonly ITracer tracer = Tracer.Get<Host>();
        private static IDisposable disposable;

        /// <summary>
        /// Registers the package components such as commands, filter, options, etc.
        /// with the development environment.
        /// </summary>
        public static void Initialize(IServiceProvider hostingPackage)
        {
            Initialize(hostingPackage, null);
        }

        /// <summary>
        /// Initializes the hosting facilities for the given package, 
        /// as well as registering the package components such as 
        /// commands, filter, options, etc.
        /// </summary>
        /// <remarks>
        /// The returned instance must remain alive while the hosting 
        /// package is loaded, ensuring that the components don't get 
        /// garbage-collected.
        /// </remarks>
        /// <param name="hostingPackage">The package owning this deploy 
        /// of Clide.</param>
        public static IDisposable Initialize(IServiceProvider hostingPackage, string tracingPaneTitle)
        {
            try
            {
                using (tracer.StartActivity("Initializing package"))
                {
                    var tracingPaneId = GetPackageGuidOrThrow(hostingPackage);
                    var devEnv = DevEnv.Get(hostingPackage);
                    // Brings in imports that the package itself might need.
                    devEnv.CompositionService.SatisfyImportsOnce(hostingPackage);

                    // Initialize the host package components.
                    var host = new Host(hostingPackage);
                    devEnv.CompositionService.SatisfyImportsOnce(host);

                    host.Initialize(tracingPaneId, tracingPaneTitle);

                    tracer.Info("Package initialization finished successfully");

                    return host;
                }
            }
            catch (Exception ex)
            {
                tracer.Error(ex, Strings.Host.FailedToInitialize);
                throw;
            }
        }

        private static Guid GetPackageGuidOrThrow(IServiceProvider owningPackage)
        {
            var guid = owningPackage.GetType().GetCustomAttributes(typeof(GuidAttribute), true)
                .OfType<GuidAttribute>()
                .FirstOrDefault();

            if (guid == null)
                throw new ArgumentException(Strings.General.MissingGuidAttribute(owningPackage.GetType()));

            return new Guid(guid.Value);
        }

        private IServiceProvider hostingPackage;

        private Host(IServiceProvider hostingPackage)
        {
            this.hostingPackage = hostingPackage;
        }

#pragma warning disable 0649
        [Import]
        private ICommandManager commands;
        [Import]
        private IOptionsManager options;
        [Import]
        private IShellEvents shellEvents;
#pragma warning restore 0649

        private void Initialize(Guid tracingPaneId, string tracingPaneTitle)
        {
            Initialize();

            if (!string.IsNullOrEmpty(tracingPaneTitle))
            {
                // We keep the instance around so that the event handlers 
                // aren't disposed.
                disposable = new TraceOutputWindowManager(
                    this.hostingPackage,
                    this.shellEvents,
                    Tracer.Manager,
                    tracingPaneId,
                    tracingPaneTitle);
            }
        }

        private void Initialize()
        {
            tracer.Info("Registering package commands");
            this.commands.AddCommands(hostingPackage);
            tracer.Info("Registering package command filters");
            this.commands.AddFilters(hostingPackage);
            tracer.Info("Registering package options pages");
            this.options.AddPages(hostingPackage);
        }

        public void Dispose()
        {
            // Do nothing for now.
        }
    }
}
