namespace Clide
{
    using System;

    /// <summary>
    /// Allows marshaling calls to the developer environment main UI thread.
    /// </summary>
    public interface IUIThread
    {
        /// <summary>
        /// Invokes the specified action in the UI thread.
        /// </summary>
        void Invoke(Action action);

        /// <summary>
        /// Invokes the specified function in the UI thread.
        /// </summary>
        TResult Invoke<TResult>(Func<TResult> function);
    }
}
