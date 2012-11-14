namespace System.Diagnostics
{
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
}
