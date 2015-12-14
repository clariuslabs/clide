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
	using Clide.Commands;
	using Clide.CommonComposition;
	using Clide.Composition;
	using Clide.Diagnostics;
	using Clide.Events;
	using Clide.Patterns.Adapter;
	using Clide.Properties;
	using System;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Windows.Threading;

    /// <summary>
    /// Core host implementation, to be cached while the 
    /// hosting package remains loaded, to prevent 
    /// garbage collection of the exposed components and 
    /// services.
    /// </summary>
    public static class Host
    {
        private static readonly ITracer tracer = Tracer.Get(typeof(Host));

        /// <summary>
        /// Registers the package components such as commands, filter, options, etc.
        /// with the development environment. This call should always be made from 
        /// the package Initialize method, which is guaranteed to run on the UI 
        /// thread.
        /// </summary>
        /// <param name="hostingPackage">The package owning this deploy 
        /// of Clide.</param>
        public static IDevEnv Initialize(IServiceProvider hostingPackage)
        {
            try
            {
                // This call should always be made from a package Initialize method, 
                // which is guaranteed to be called from the UI thread.
                UIThread.Initialize(Dispatcher.CurrentDispatcher);

                var devEnv = DevEnv.Get(hostingPackage);
                using (tracer.StartActivity("Initializing package"))
                {
                    // TODO
                    // Brings in imports that the package itself might need.
                    //devEnv.ServiceLocator.SatisfyImportsOnce(hostingPackage);

                    // Initialize the host package components.
                    var host = devEnv.ServiceLocator.GetInstance<HostImpl>();
                    host.Initialize();

					// This call causes the static initialization on Adapters to run, which 
					// is then overriden on the next line.
					Debug.Assert(Adapters.ServiceInstance != null);
					// Re-initialize the adapter service so that extended adapter implementations
					// are available.
					AdaptersInitializer.SetService(devEnv.ServiceLocator.GetInstance<IAdapterService>());

					// Force initialization of the global service locator so that the component model 
					// is requested from the UI thread.
					// This call also forces initialization of the global service locator singleton.
					Debug.Assert(ServiceLocator.GlobalLocator != null);

                    tracer.Info("Package initialization finished successfully");

                    return devEnv;
                }
            }
            catch (Exception ex)
            {
                tracer.Error(ex, Strings.Host.FailedToInitialize);
                throw;
            }
        }

        /// <summary>
        /// Obsolete
        /// </summary>
        [Obsolete(@"See http://clarius.io/clide-tracing for more information.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IDisposable Initialize(IServiceProvider hostingPackage, string tracingPaneTitle, string rootTraceSource)
        {
            throw new NotSupportedException();
        }
    }

    [Component(IsSingleton = true)]
    internal class HostImpl
    {
        private static readonly ITracer tracer = Tracer.Get<HostImpl>();
        private readonly ICommandManager commands;
        private readonly IOptionsManager options;

        public HostImpl(ICommandManager commands, IOptionsManager options)
        {
            this.commands = commands;
            this.options = options;
        }

        internal void Initialize()
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
    }
}
