﻿#region BSD License
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

namespace Clide.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using Clide.Events;
    using Clide.Properties;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    ///  Manages the output of trace messages to an output window pane.
    /// </summary>
    public sealed class TraceOutputWindowManager : IDisposable
    {
        private static readonly ITracer tracer = Tracer.Get<TraceOutputWindowManager>();

        private IServiceProvider serviceProvider;
        private IShellEvents shellEvents;
        private Lazy<IUIThread> uiThread;
        private ITracerManager tracerManager;

        private IVsOutputWindowPane outputWindowPane;
        private Guid outputPaneGuid;
        private string outputPaneTitle;

        private TraceListener listener;
        private StringWriter temporaryWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceOutputWindowManager"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="shellEvents">The shell events.</param>
        /// <param name="outputPaneId">The output pane GUID, which must be unique and remain constant for a given pane.</param>
        /// <param name="outputPaneTitle">The output pane title.</param>
        /// <param name="rootTraceSource">Root trace source to hook the output window trace listener to.</param>
        public TraceOutputWindowManager(IServiceProvider serviceProvider, IShellEvents shellEvents,
            Lazy<IUIThread> uiThread, ITracerManager tracerManager, Guid outputPaneId, string outputPaneTitle, string rootTraceSource = TracerManager.DefaultSourceName)
        {
            Guard.NotNull(() => serviceProvider, serviceProvider);
            Guard.NotNull(() => shellEvents, shellEvents);
            Guard.NotNull(() => uiThread, uiThread);            
            Guard.NotNull(() => tracerManager, tracerManager);
            Guard.NotNullOrEmpty(() => outputPaneTitle, outputPaneTitle);

            this.serviceProvider = serviceProvider;
            this.shellEvents = shellEvents;
            this.uiThread = uiThread;
            this.tracerManager = tracerManager;

            this.outputPaneGuid = outputPaneId;
            this.outputPaneTitle = outputPaneTitle;
            this.rootTraceSource = rootTraceSource;

            // Create a temporary writer that buffers events that happen 
            // before shell initialization is completed, so that we don't 
            // miss anything.
            this.temporaryWriter = new StringWriter(CultureInfo.CurrentCulture);
            this.listener = new IndentingTextListener(this.temporaryWriter, this.outputPaneTitle);
            this.listener.IndentLevel = 4;

            this.tracerManager.AddListener(rootTraceSource, this.listener);

            this.shellEvents.Initialized += this.OnShellInitialized;
        }

        /// <summary>
        /// Cleans resources used by the manager.
        /// </summary>
        public void Dispose()
        {
            this.tracerManager.RemoveListener(this.rootTraceSource, this.listener);
            this.listener.Dispose();
            this.listener = null;

            if (this.temporaryWriter != null)
            {
                this.temporaryWriter.Dispose();
            }
        }

        private void OnShellInitialized(object sender, EventArgs args)
        {
            using (tracer.StartActivity("Initializing trace output window"))
            {
                this.EnsureOutputWindow();
                this.listener.Flush();

                var outputWriter = new OutputWindowTextWriter(this.uiThread, this.outputWindowPane);

                // Replace temporary listener with the proper one, populating the 
                // output window from the temporary buffer.
                var tempLog = this.temporaryWriter.ToString();

                if (!string.IsNullOrEmpty(tempLog))
                    outputWriter.WriteLine(tempLog);

                this.temporaryWriter = null;

                // Remove existing listener which writes to the temporary writer.
                this.tracerManager.RemoveListener(this.rootTraceSource, this.listener);

                // Initialize the true listener that writes to the output window.
                this.listener = new IndentingTextListener(outputWriter, this.outputPaneTitle);
                this.listener.IndentLevel = 4;

                this.tracerManager.AddListener(this.rootTraceSource, this.listener);

                tracer.Info("Trace output window initialized successfully");
            }
        }

        private void EnsureOutputWindow()
        {
            if (this.outputWindowPane == null)
            {
                tracer.Info("Creating trace output window");
                var outputWindow = (IVsOutputWindow)this.serviceProvider.GetService(typeof(SVsOutputWindow));
                tracer.ShieldUI(() =>
                {
                    if (!ErrorHandler.Succeeded(outputWindow.GetPane(ref this.outputPaneGuid, out this.outputWindowPane)) || this.outputWindowPane == null)
                    {
                        ErrorHandler.ThrowOnFailure(outputWindow.CreatePane(ref this.outputPaneGuid, this.outputPaneTitle, 1, 1));
                        ErrorHandler.ThrowOnFailure(outputWindow.GetPane(ref this.outputPaneGuid, out this.outputWindowPane));
                    }
                },
                Strings.Diagnostics.FailedToCreateOutputWindow);
                tracer.Info("Trace output window created");
            }
        }

        public string rootTraceSource { get; set; }
    }
}
