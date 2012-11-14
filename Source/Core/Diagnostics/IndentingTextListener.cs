namespace Clide.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.IO;

    /// <summary>
    /// A <see cref="TextWriterTraceListener"/> that indents traces 
    /// whenver a <see cref="TraceEventType.Start"/> is issued.
    /// </summary>
    public class IndentingTextListener : TextWriterTraceListener
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TraceRecordTextListener"/> class.
        /// </summary>
        public IndentingTextListener(TextWriter writer)
            : base(writer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceRecordTextListener"/> class.
        /// </summary>
        public IndentingTextListener(TextWriter writer, string name)
            : base(writer, name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndentingTextListener"/> class.
        /// </summary>
        public IndentingTextListener(string fileName)
            : base(fileName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndentingTextListener"/> class.
        /// </summary>
        public IndentingTextListener(string fileName, string name)
            : base(fileName, name)
        {
        }

        /// <summary>
        /// Writes trace and event information to the listener specific output.
        /// </summary>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            this.SetIndent(eventType, () => base.TraceEvent(eventCache, source, eventType, id));
        }

        /// <summary>
        /// Writes trace information, a formatted array of objects and event information to the listener specific output.
        /// </summary>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            this.SetIndent(eventType, () => base.TraceEvent(eventCache, source, eventType, id, format, args));
        }

        /// <summary>
        /// Writes trace information, a message, and event information to the listener specific output.
        /// </summary>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            this.SetIndent(eventType, () => base.TraceEvent(eventCache, source, eventType, id, message));
        }

        /// <summary>
        /// Writes trace information, a data object and event information to the listener specific output.
        /// </summary>
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            this.SetIndent(eventType, () => base.TraceData(eventCache, source, eventType, id, data));
        }

        private void SetIndent(TraceEventType eventType, Action action)
        {
            if (eventType == TraceEventType.Stop)
            {
                base.IndentLevel--;
                base.NeedIndent = base.IndentLevel > 0;
            }

            action();

            if (eventType == TraceEventType.Start)
            {
                base.IndentLevel++;
                base.NeedIndent = true;
            }
        }
    }
}
