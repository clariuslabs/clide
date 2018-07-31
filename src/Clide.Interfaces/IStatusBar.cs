namespace Clide
{
    /// <summary>
    /// Provides membes to interact with the status bar.
    /// </summary>
	public interface IStatusBar : IFluentInterface
    {
        /// <summary>
        /// Clears the message in the status bar.
        /// </summary>
		void Clear();

        /// <summary>
        /// Shows the given message in the status bar.
        /// </summary>
        /// <param name="message">The message to show.</param>
		void ShowMessage(string message);

        /// <summary>
        /// Shows the given progress message and value.
        /// </summary>
        /// <param name="message">The message to show.</param>
        /// <param name="complete">The completed value so far.</param>
        /// <param name="total">The total value to be completed.</param>
		void ShowProgress(string message, int complete, int total);
    }
}