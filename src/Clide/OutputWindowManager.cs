#region BSD License
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

namespace Clide
{
	using Clide.CommonComposition;
	using Clide.Diagnostics;
	using Clide.Events;
	using Clide.Properties;
	using Microsoft.VisualStudio;
	using Microsoft.VisualStudio.Shell.Interop;
	using System;
	using System.Collections.Concurrent;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Diagnostics;

	/// <summary>
	///  Manages the output of trace messages to an output window pane.
	/// </summary>
	[Component(IsSingleton = true)]
    internal class OutputWindowManager : IOutputWindowManager
    {
        private static readonly ITracer tracer = Tracer.Get<OutputWindowManager>();

        private ConcurrentDictionary<Guid, TextWriter> writerCache = new ConcurrentDictionary<Guid, TextWriter>();

        private IServiceProvider serviceProvider;
        private IShellEvents shellEvents;
        private Lazy<IUIThread> uiThread;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputWindowManager" /> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="shellEvents">The shell events.</param>
        /// <param name="uiThread">The UI thread.</param>
        public OutputWindowManager(IServiceProvider serviceProvider, IShellEvents shellEvents, Lazy<IUIThread> uiThread)
        {
            Guard.NotNull(() => serviceProvider, serviceProvider);
            Guard.NotNull(() => shellEvents, shellEvents);
            Guard.NotNull(() => uiThread, uiThread);

            this.serviceProvider = serviceProvider;
            this.shellEvents = shellEvents;
            this.uiThread = uiThread;
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

            shellEvents.Initialized += (sender, args) =>
            {
                using (tracer.StartActivity(Strings.OutputWindowManager.TraceInitializing(title)))
                {
                    IVsOutputWindowPane pane = GetVsPane(id, title);
                    if (pane != null)
                    {
                        var outputWriter = new OutputWindowTextWriter(uiThread, pane);

                        // Dump over the cached text from the initial writer.
                        stringWriter.Flush();
                        outputWriter.Write(stringWriter.ToString());

                        // Replace the strategy, which will now write directly to the output pane.
                        writer.StrategyWriter = outputWriter;
                    }
                }
            };

            return writer;
        }

        private IVsOutputWindowPane GetVsPane(Guid id, string title)
        {
            IVsOutputWindowPane pane = null;

            tracer.ShieldUI(() =>
            {
                tracer.Verbose(Strings.OutputWindowManager.RetrievingPane(title));

                var outputWindow = this.serviceProvider.GetService<SVsOutputWindow, IVsOutputWindow>();
                if (!ErrorHandler.Succeeded(outputWindow.GetPane(ref id, out pane)))
                {
                    tracer.Verbose(Strings.OutputWindowManager.CreatingPane(title));
                    
                    ErrorHandler.ThrowOnFailure(outputWindow.CreatePane(ref id, title, 1, 1));
                    ErrorHandler.ThrowOnFailure(outputWindow.GetPane(ref id, out pane));
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
            private Lazy<IUIThread> uiThread;
            private IVsOutputWindowPane outputPane;

            public OutputWindowTextWriter(Lazy<IUIThread> uiThread, IVsOutputWindowPane outputPane)
            {
                this.uiThread = uiThread;
                this.outputPane = outputPane;
            }

            public override Encoding Encoding
            {
                get { return Encoding.UTF8; }
            }

            public override void Write(string value)
            {
                uiThread.Value.BeginInvoke(() => outputPane.OutputStringThreadSafe(value));
            }

            public override void WriteLine()
            {
                uiThread.Value.BeginInvoke(() => outputPane.OutputStringThreadSafe(Environment.NewLine));
            }

            public override void WriteLine(string value)
            {
                Write(value); ;
                WriteLine();
            }
        }
    }
}
