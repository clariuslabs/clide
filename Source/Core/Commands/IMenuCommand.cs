namespace Clide.Commands
{
    /// <summary>
    /// Provides current status information for an <see cref="ICommandExtension"/> 
    /// and <see cref="ICommandFilter"/>.
    /// </summary>
    public interface IMenuCommand
    {
        /// <summary>
        /// Gets or sets a value indicating whether a <see cref="ICommandExtension"/> is enabled.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the text to display for the command.
        /// </summary>
        string Text { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ICommandExtension"/> is visible.
        /// </summary>
        bool Visible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this menu item is checked.
        /// </summary>
        bool Checked { get; set; }
    }
}
