namespace Clide
{
	/// <summary>
	/// Common interface for tool windows.
	/// </summary>
	public interface IToolWindow : IFluentInterface
	{
        /// <summary>
        /// Whether the tool window is currently visible.
        /// </summary>
		bool IsVisible { get; }

        /// <summary>
        /// Makes the tool window visible if it was closed or hidden.
        /// </summary>
		void Show();

        /// <summary>
        /// Closes the tool window.
        /// </summary>
		void Close();
	}
}
