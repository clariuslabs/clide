namespace Clide
{
    /// <summary>
    /// Provides a contract to show messages to the user.
    /// </summary>
    public interface IMessageBoxService
    {
        /// <summary>
        /// Shows a message to the user.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the user clicked on Yes/OK, 
        /// <see langword="false"/> if the user clicked No, 
        /// <see langword="null"/> if the user cancelled the dialog or clicked 
        /// <c>Cancel</c> or any other value other than the Yes/OK/No.
        /// </returns>
        bool? Show(string message, string title = MessageBoxServiceDefaults.DefaultTitle, MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None, MessageBoxResult defaultResult = MessageBoxResult.OK);

        /// <summary>
        /// Prompts the user for a response.
        /// </summary>
        MessageBoxResult Prompt(string message, string title = MessageBoxServiceDefaults.DefaultTitle, MessageBoxButton button = MessageBoxButton.OKCancel, MessageBoxImage icon = MessageBoxImage.Question, MessageBoxResult defaultResult = MessageBoxResult.OK);
    }

    public class MessageBoxServiceDefaults
    {
        public const string DefaultTitle = "Microsoft Visual Studio";
        public const MessageBoxButton DefaultButton = MessageBoxButton.OK;
        public const MessageBoxImage DefaultIcon = MessageBoxImage.None;
        public const MessageBoxResult DefaultResult = MessageBoxResult.OK;
    }
}
