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
using Microsoft.VisualStudio.Threading;

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
        readonly JoinableTaskFactory jtf;

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
            JoinableTaskContext context)
        {
            this.vsOutputWindow = vsOutputWindow;
            this.eventStream = eventStream;
            jtf = context.Factory;
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
                        var outputWriter = new OutputWindowTextWriter(jtf, pane);

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
                jtf.Run(async () =>
                {
                    await jtf.SwitchToMainThreadAsync();
                    if (!ErrorHandler.Succeeded(vsOutputWindow.Value.GetPane(ref id, out pane)))
                    {
                        tracer.Verbose(Strings.OutputWindowManager.CreatingPane(title));

                        ErrorHandler.ThrowOnFailure(vsOutputWindow.Value.CreatePane(ref id, title, 1, 1));
                        ErrorHandler.ThrowOnFailure(vsOutputWindow.Value.GetPane(ref id, out pane));
                    }
                });
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
            private readonly JoinableTaskFactory jtf;
            private IVsOutputWindowPane outputPane;

            public OutputWindowTextWriter(JoinableTaskFactory jtf, IVsOutputWindowPane outputPane)
            {
                this.jtf = jtf;
                this.outputPane = outputPane;
            }

            public override Encoding Encoding
            {
                get { return Encoding.UTF8; }
            }

            public override void Write(string value)
            {
                jtf.RunAsync(async () =>
                {
                    await jtf.SwitchToMainThreadAsync();
                    outputPane.OutputStringThreadSafe(value);
                });
            }

            public override void WriteLine()
            {
                jtf.RunAsync(async () =>
                {
                    await jtf.SwitchToMainThreadAsync();
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