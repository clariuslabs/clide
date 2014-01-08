using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Clide.Diagnostics
{
    /// <summary>
    /// A custom <see cref="TraceListener"/> that writes simple text rendering for traces.
    /// </summary>
    public class TextTraceListener : TraceListener
    {
        // "Information".Length
        private const int MaxEventTypeLength = 11;
        private TextWriter writer;

        private static readonly TraceEventType[] ignoredEvents = new[] { TraceEventType.Resume, TraceEventType.Stop, TraceEventType.Suspend, TraceEventType.Transfer };

        /// <summary>
        /// Initializes a new instance of the <see cref="TextTraceListener"/> class.
        /// </summary>
        /// <param name="writer">The writer to write traces to.</param>
        public TextTraceListener(TextWriter writer)
        {
            this.writer = writer;
        }

        /// <summary>
        /// Writes trace information, a message, and event information to the listener specific output.
        /// </summary>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            if ((this.Filter == null) || this.Filter.ShouldTrace(eventCache, source, eventType, id, message, null, null, null))
            {
                DoTrace(eventCache, source, eventType, message);
            }
        }

        /// <summary>
        /// Writes trace information, a formatted array of objects and event information to the listener specific output.
        /// </summary>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            if ((this.Filter == null) || this.Filter.ShouldTrace(eventCache, source, eventType, id, format, args, null, null))
            {
                if (args != null)
                {
                    DoTrace(eventCache, source, eventType, string.Format(CultureInfo.InvariantCulture, format, args));
                }
                else
                {
                    DoTrace(eventCache, source, eventType, format);
                }
            }
        }

        /// <summary>
        /// Always skips writing data traces.
        /// </summary>
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
        }

        /// <summary>
        /// Always skips writing data traces.
        /// </summary>
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
        }

        /// <summary>
        /// Always skips writing event traces without messages.
        /// </summary>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
        }

        /// <summary>
        /// Always skips writing transfer traces.
        /// </summary>
        public override void TraceTransfer(TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId)
        {
        }

        /// <summary>
        /// Writes the specified message to the underlying <see cref="TextWriter"/>.
        /// </summary>
        public override void Write(string message)
        {
            writer.Write(message);
        }

        /// <summary>
        /// Writes the specified message to the underlying <see cref="TextWriter"/>, followed by a line terminator.
        /// </summary>
        public override void WriteLine(string message)
        {
            writer.WriteLine(message);
        }

        /// <summary>
        /// After determining if tracing should be performed according to the configured <see cref="Filter"/>, 
        /// issues the <see cref="WriteLine"/> call.
        /// </summary>
        /// <param name="eventCache">The event cache that contains extra information about the trace context.</param>
        /// <param name="source">The trace source name that issued the trace.</param>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="message">The message to trace.</param>
        /// <remarks>
        /// <see cref="TraceEventType.Stop"/> traces are ignored, as well as 
        /// </remarks>
        protected virtual void DoTrace(TraceEventCache eventCache, string source, TraceEventType eventType, string message)
        {
            if (ignoredEvents.Contains(eventType))
                return;

            WriteLine(eventType.ToString().PadRight(MaxEventTypeLength) + ": " + message);
        }
    }
}