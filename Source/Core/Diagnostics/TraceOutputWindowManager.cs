namespace Clide.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using Clide.Properties;

    /// <summary>
    ///  Manages the output of trace messages to an output window pane.
    /// </summary>
    public sealed class TraceOutputWindowManager : IDisposable
    {
        private static readonly ITracer tracer = Tracer.Get<TraceOutputWindowManager>();

        private IServiceProvider serviceProvider;
        private IShellEvents shellEvents;
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
        public TraceOutputWindowManager(IServiceProvider serviceProvider, IShellEvents shellEvents, ITracerManager tracerManager, Guid outputPaneId, string outputPaneTitle)
        {
            Guard.NotNull(() => serviceProvider, serviceProvider);
            Guard.NotNull(() => shellEvents, shellEvents);
            Guard.NotNull(() => tracerManager, tracerManager);
            Guard.NotNullOrEmpty(() => outputPaneTitle, outputPaneTitle);

            this.serviceProvider = serviceProvider;
            this.outputPaneGuid = outputPaneId;
            this.outputPaneTitle = outputPaneTitle;
            this.shellEvents = shellEvents;
            this.tracerManager = tracerManager;

            // Create a temporary writer that buffers events that happen 
            // before shell initialization is completed, so that we don't 
            // miss anything.
            this.temporaryWriter = new StringWriter(CultureInfo.CurrentCulture);
            this.listener = new IndentingTextListener(this.temporaryWriter, this.outputPaneTitle);
            this.listener.IndentLevel = 4;

            this.tracerManager.AddListener(TracerManager.DefaultSourceName, this.listener);

            this.shellEvents.Initialized += this.OnShellInitialized;
        }

        /// <summary>
        /// Cleans resources used by the manager.
        /// </summary>
        public void Dispose()
        {
            this.tracerManager.RemoveListener(TracerManager.DefaultSourceName, this.listener);
            this.listener.Dispose();
            this.listener = null;

            if (this.temporaryWriter != null)
            {
                this.temporaryWriter.Dispose();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "We dispose the listener, which disposes the internal writer.")]
        private void OnShellInitialized(object sender, EventArgs args)
        {
            this.EnsureOutputWindow();

            this.listener.Flush();
            // Replace temporary listener with the proper one, populating the 
            // output window from the temporary buffer.
            var tempLog = this.temporaryWriter.ToString();

            if (!string.IsNullOrEmpty(tempLog))
            {
                ErrorHandler.ThrowOnFailure(this.outputWindowPane.OutputStringThreadSafe(this.temporaryWriter.ToString()));
            }

            this.temporaryWriter = null;

            // Remove existing listener which writes to the temporary writer.
            this.tracerManager.RemoveListener(TracerManager.DefaultSourceName, this.listener);

            // Initialize the true listener that writes to the output window.
            this.listener = new IndentingTextListener(new OutputWindowTextWriter(this.outputWindowPane), this.outputPaneTitle);
            this.listener.IndentLevel = 4;

            this.tracerManager.AddListener(TracerManager.DefaultSourceName, this.listener);
        }

        private void EnsureOutputWindow()
        {
            if (this.outputWindowPane == null)
            {
                var outputWindow = (IVsOutputWindow)this.serviceProvider.GetService(typeof(SVsOutputWindow));
                tracer.ShieldUI(() =>
                {
                    ErrorHandler.ThrowOnFailure(outputWindow.CreatePane(ref this.outputPaneGuid, this.outputPaneTitle, 1, 1));
                    ErrorHandler.ThrowOnFailure(outputWindow.GetPane(ref this.outputPaneGuid, out this.outputWindowPane));
                },
                Strings.Diagnostics.FailedToCreateOutputWindow);
            }
        }
    }
}
