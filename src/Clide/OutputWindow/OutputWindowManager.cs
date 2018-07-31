using Clide.Properties;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.ComponentModel.Composition;
using Merq;
using Clide.Events;
namespace Clide
{

    [Export(typeof(IOutputWindowManager))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class OutputWindowManager : IOutputWindowManager
    {
        static readonly ITracer tracer = Tracer.Get<OutputWindowManager>();

        ConcurrentDictionary<Guid, TextWriter> writerCache = new ConcurrentDictionary<Guid, TextWriter>();

        readonly Lazy<IVsOutputWindow> vsOutputWindow;
        readonly Lazy<IEventStream> eventStream;
        readonly Lazy<IAsyncManager> asyncManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputWindowManager" /> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="shellEvents">The shell events.</param>
        /// <param name="uiThread">The UI thread.</param>
        [ImportingConstructor]
        public OutputWindowManager(
            [Import(ContractNames.Interop.VsOutputWindow)] Lazy<IVsOutputWindow> vsOutputWindow,
            Lazy<IEventStream> eventStream,
            Lazy<IAsyncManager> asyncManager)
        {
            this.vsOutputWindow = vsOutputWindow;
            this.eventStream = eventStream;
            this.asyncManager = asyncManager;
        }

        /// <summary>
        /// Cleans resources used by the manager.
        /// </summary>
        public void Dispose()
        {
            foreach (var writer in writerCache.Values.ToArray())
            {
                writer.Flush();
                writer.Dispose();
            }

            writerCache.Clear();
        }

        public TextWriter GetPane(Guid id, string title)
        {
            return writerCache.GetOrAdd(id, CreateWriter(id, title));
        }

        private TextWriter CreateWriter(Guid id, string title)
        {
            var stringWriter = new StringWriter();
            var writer = new StrategyTextWriter(stringWriter);

            eventStream.Value.Of<ShellInitialized>().Subscribe(_ =>
            {
                using (tracer.StartActivity(Strings.OutputWindowManager.TraceInitializing(title)))
                {
                    IVsOutputWindowPane pane = GetVsPane(id, title);
                    if (pane != null)
                    {
                        var outputWriter = new OutputWindowTextWriter(asyncManager, pane);

                        // Dump over the cached text from the initial writer.
                        stringWriter.Flush();
                        outputWriter.Write(stringWriter.ToString());

                        // Replace the strategy, which will now write directly to the output pane.
                        writer.StrategyWriter = outputWriter;
                    }
                }
            });

            return writer;
        }

        private IVsOutputWindowPane GetVsPane(Guid id, string title)
        {
            IVsOutputWindowPane pane = null;

            tracer.ShieldUI(() =>
            {
                tracer.Verbose(Strings.OutputWindowManager.RetrievingPane(title));

                if (!ErrorHandler.Succeeded(vsOutputWindow.Value.GetPane(ref id, out pane)))
                {
                    tracer.Verbose(Strings.OutputWindowManager.CreatingPane(title));

                    ErrorHandler.ThrowOnFailure(vsOutputWindow.Value.CreatePane(ref id, title, 1, 1));
                    ErrorHandler.ThrowOnFailure(vsOutputWindow.Value.GetPane(ref id, out pane));
                }
            }, Strings.OutputWindowManager.FailedToCreatePane(title));

            return pane;
        }

        private class StrategyTextWriter : TextWriter
        {
            public StrategyTextWriter(TextWriter initialStrategy)
            {
                StrategyWriter = initialStrategy;
            }

            public TextWriter StrategyWriter { get; set; }

            public override void Write(string value)
            {
                StrategyWriter.Write(value);
            }

            public override void WriteLine()
            {
                StrategyWriter.Write(Environment.NewLine);
            }

            public override void WriteLine(string value)
            {
                StrategyWriter.WriteLine(value);
            }

            public override Encoding Encoding
            {
                get { return StrategyWriter.Encoding; }
            }
        }

        private class OutputWindowTextWriter : TextWriter
        {
            private Lazy<IAsyncManager> asyncManager;
            private IVsOutputWindowPane outputPane;

            public OutputWindowTextWriter(Lazy<IAsyncManager> asyncManager, IVsOutputWindowPane outputPane)
            {
                this.asyncManager = asyncManager;
                this.outputPane = outputPane;
            }

            public override Encoding Encoding
            {
                get { return Encoding.UTF8; }
            }

            public override void Write(string value)
            {
                asyncManager.Value.RunAsync(async () =>
                {
                    await asyncManager.Value.SwitchToMainThread();
                    outputPane.OutputStringThreadSafe(value);
                });
            }

            public override void WriteLine()
            {
                asyncManager.Value.RunAsync(async () =>
                {
                    await asyncManager.Value.SwitchToMainThread();
                    outputPane.OutputStringThreadSafe(Environment.NewLine);
                });
            }

            public override void WriteLine(string value)
            {
                Write(value);
                WriteLine();
            }
        }
    }
}