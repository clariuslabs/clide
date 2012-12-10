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
    // Changes to this file will not be overwritten by NuGet when updating.
    // To make the entire API public, just uncomment the following code:

    public partial interface ITracer { }
    public static partial class ITracerExtensions { }
    public partial interface ITracerManager { }
    public static partial class Tracer { }

    // Further customizations here.
    public static partial class StartActivityExtension { }

    /// <devdoc>
    /// Exposes the trace manager internally.
    /// </devdoc>
    partial class Tracer
    {
        /// <summary>
        /// Gets the trace manager to manipulate the tracing level and listeners.
        /// </summary>
        public static ITracerManager Manager { get { return manager; } }

        // Implement missing members that we added.
        partial class DefaultManager : ITracerManager
        {
            public void AddListener(string sourceName, TraceListener listener)
            {
            }

            public void RemoveListener(string sourceName, TraceListener listener)
            {
            }

            public void RemoveListener(string sourceName, string listenerName)
            {
            }

            public void SetTracingLevel(string sourceName, SourceLevels level)
            {
            }

            public TraceSource GetSource(string sourceName)
            {
                throw new NotSupportedException();
            }
        }
    }

    /// <devdoc>
    /// Adds our tracer configuration members.
    /// </devdoc>
    partial interface ITracerManager
    {
        /// <summary>
        /// Adds a listener to the source with the given <paramref name="sourceName"/>.
        /// </summary>
        void AddListener(string sourceName, TraceListener listener);

        /// <summary>
        /// Removes a listener from the source with the given <paramref name="sourceName"/>.
        /// </summary>
        void RemoveListener(string sourceName, TraceListener listener);

        /// <summary>
        /// Removes a listener from the source with the given <paramref name="sourceName"/>.
        /// </summary>
        void RemoveListener(string sourceName, string listenerName);

        /// <summary>
        /// Sets the tracing level for the source with the given <paramref name="sourceName"/>
        /// </summary>
        void SetTracingLevel(string sourceName, SourceLevels level);

        /// <summary>
        /// Gets the underlying trace source of the given name.
        /// </summary>
        TraceSource GetSource(string sourceName);
    }
}
