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
        /// Initializes a new instance of the <see cref="IndentingTextListener"/> class.
        /// </summary>
        public IndentingTextListener(TextWriter writer)
            : base(writer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndentingTextListener"/> class.
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
