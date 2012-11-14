namespace Clide
{
    using System;

    /// <summary>
    /// Shell-related events and state.
    /// </summary>
    public interface IShellEvents
    {
        /// <summary>
        /// Gets a value indicating whether the shell has been initialized.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Occurs when the shell has finished initializing.
        /// </summary>
        event EventHandler Initialized;
    }
}
