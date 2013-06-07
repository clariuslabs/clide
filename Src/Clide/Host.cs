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
    using System.ComponentModel.Composition.Hosting;
    using Clide.Patterns.Adapter;
    using Clide.Events;
    using System.Windows.Threading;
    using Clide.Composition;

    [Component]
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
            Initialize(hostingPackage, null, null);
        }

        /// <summary>
        /// Initializes the hosting facilities for the given package, 
        /// as well as registering the package components such as 
        /// commands, filter, options, etc.
        /// </summary>
        /// <remarks>
        /// The returned instance must remain alive while the hosting 
        /// package is loaded, ensuring that the components don't get 
        /// garbage-collected and in particular for the tracing the 
        /// output window to remain active.
        /// </remarks>
        /// <param name="hostingPackage">The package owning this deploy 
        /// of Clide.</param>
        /// <param name="tracingPaneTitle">Optional title of an output 
        /// pane to create for the calling package, for tracing purposes. 
        /// If it</param>
        /// <param name="rootTraceSource">Root trace source to hook the output window trace listener to.</param>
        public static IDisposable Initialize(IServiceProvider hostingPackage, string tracingPaneTitle, string rootTraceSource)
        {
            try
            {
                using (tracer.StartActivity("Initializing package"))
                {
                    // This call should always be made from a package Initialize method, 
                    // which is guaranteed to be called from the UI thread.
                    UIThread.Initialize(Dispatcher.CurrentDispatcher);
                    var packageId = hostingPackage.GetPackageGuidOrThrow();
                    var devEnv = DevEnv.Get(hostingPackage);
                    // Brings in imports that the package itself might need.

                    // TODO
                    //devEnv.ServiceLocator.SatisfyImportsOnce(hostingPackage);

                    // Initialize the host package components.
                    var host = devEnv.ServiceLocator.GetInstance<Host>();
                    host.Initialize(packageId, tracingPaneTitle, rootTraceSource);

                    // Initialize the default adapter service for the smart cast extension method.
                    Clide.Patterns.Adapter.AdaptersInitializer.SetService(devEnv.ServiceLocator.GetInstance<IAdapterService>());

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

        private readonly IServiceProvider hostingPackage;
        private readonly ICommandManager commands;
        private readonly IOptionsManager options;
        private readonly IShellEvents shellEvents;
        private readonly Lazy<IUIThread> uiThread;

        private Host(IServiceProvider hostingPackage, ICommandManager commands, IOptionsManager options, IShellEvents shellEvents, Lazy<IUIThread> uiThread)
        {
            this.hostingPackage = hostingPackage;
            this.commands = commands;
            this.options = options;
            this.shellEvents = shellEvents;
            this.uiThread = uiThread;
        }

        private void Initialize(Guid packageId, string tracingPaneTitle, string rootTraceSource)
        {
            Initialize();

            if (!string.IsNullOrEmpty(tracingPaneTitle) && !string.IsNullOrEmpty(rootTraceSource))
            {
                // We keep the instance around so that the event handlers 
                // aren't disposed.
                disposable = new TraceOutputWindowManager(
                    hostingPackage,
                    shellEvents,
                    uiThread,
                    Tracer.Manager,
                    packageId,
                    tracingPaneTitle,
                    rootTraceSource);
            }
        }

        private void Initialize()
        {
            tracer.Info("Registering package commands");
            this.commands.AddCommands();
            tracer.Info("Registering package command filters");
            this.commands.AddFilters();
            tracer.Info("Registering package command interceptors");
            this.commands.AddInterceptors();
            tracer.Info("Registering package options pages");
            this.options.AddPages();
        }

        public void Dispose()
        {
            // Do nothing for now.
        }
    }
}
