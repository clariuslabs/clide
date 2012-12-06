#region BSD License
/* 
Copyright (c) 2011, Clarius Consulting
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, 
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list 
  of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this 
  list of conditions and the following disclaimer in the documentation and/or other 
  materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be 
  used to endorse or promote products derived from this software without specific 
  prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY 
EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES 
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT 
SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, 
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED 
TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR 
BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN 
ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH 
DAMAGE.
*/
#endregion

namespace System.Diagnostics
{
    static partial class StartActivityExtension
    {
        /// <summary>
        /// Starts a new activity scope.
        /// </summary>
        public static IDisposable StartActivity(this ITracer tracer, string format, params object[] args)
        {
            return new TraceActivity(tracer, format, args);
        }

        /// <summary>
        /// Starts a new activity scope.
        /// </summary>
        public static IDisposable StartActivity(this ITracer tracer, string displayName)
        {
            return new TraceActivity(tracer, displayName);
        }

        /// <devdoc>
        /// In order for activity tracing to happen, the trace source needs to 
        /// have <see cref="SourceLevels.ActivityTracing"/> enabled.
        /// </devdoc>
        //[DebuggerStepThrough]
        private class TraceActivity : IDisposable
        {
            private string displayName;
            private object[] args;
            private bool disposed;
            private ITracer tracer;
            private Guid oldId;
            private Guid newId;

            public TraceActivity(ITracer tracer, string displayName)
                : this(tracer, displayName, null)
            {
            }

            public TraceActivity(ITracer tracer, string format, params object[] args)
            {
                this.displayName = format;
                if (args != null && args.Length > 0)
                    this.args = args;

                this.tracer = tracer;
                this.newId = Guid.NewGuid();
                this.oldId = Trace.CorrelationManager.ActivityId;

                if (this.oldId != Guid.Empty)
                    tracer.Trace(TraceEventType.Transfer, this.newId);

                Trace.CorrelationManager.ActivityId = newId;

                if (this.args == null)
                {
                    this.tracer.Trace(TraceEventType.Start, this.displayName);
                    //Trace.CorrelationManager.StartLogicalOperation(this.displayName);
                }
                else
                {
                    this.tracer.Trace(TraceEventType.Start, this.displayName, args);
                    //Trace.CorrelationManager.StartLogicalOperation(string.Format(displayName, args));
                }
            }

            public void Dispose()
            {
                if (!this.disposed)
                {
                    if (this.args == null)
                        this.tracer.Trace(TraceEventType.Stop, this.displayName);
                    else
                        this.tracer.Trace(TraceEventType.Stop, this.displayName, args);

                    if (this.oldId != Guid.Empty)
                        tracer.Trace(TraceEventType.Transfer, this.oldId);

                    //Trace.CorrelationManager.StopLogicalOperation();
                    Trace.CorrelationManager.ActivityId = this.oldId;
                }

                this.disposed = true;
            }
        }
    }
}
