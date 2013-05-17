namespace Clide.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Linq;

    partial class TracerManager
    {
        /// <summary>
        /// Gets the underlying <see cref="TraceSource"/> for the given name.
        /// </summary>
        public TraceSource GetSource(string name)
        {
            return GetOrAdd(name, s => CreateSource(s));
        }
    }
}