namespace Clide.Diagnostics
{
    using System;
    using System.Diagnostics;

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