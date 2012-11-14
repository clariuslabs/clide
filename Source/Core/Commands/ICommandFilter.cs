namespace Clide.Commands
{

    /// <summary>
    /// Represents a component that reacts to the command 
    /// query status allowing to filter command availability.
    /// </summary>
    public interface ICommandFilter
    {
        /// <summary>
        /// Determines the dynamic state of the command.
        /// </summary>
        void QueryStatus(IMenuCommand command);
    }
}
