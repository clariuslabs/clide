namespace Clide
{
    /// <summary>
    /// Represents a command that extends the environment.
    /// </summary>
    public interface ICommandExtension
    {
        /// <summary>
        /// Gets the initial text for the command.
        /// </summary>
        string Text { get; }

        /// <summary>
        /// Executes the command, taking into account its <see cref="IMenuCommand"/> state.
        /// </summary>
        void Execute(IMenuCommand command);

        /// <summary>
        /// Determines the dynamic state of the command.
        /// </summary>
        void QueryStatus(IMenuCommand command);
    }
}
