using System;
namespace Clide
{

    /// <summary>
    /// Provides members to interact with the Error List window
    /// </summary>
    public interface IErrorsManager
    {
        /// <summary>
        /// Adds an error to the error list.
        /// </summary>
        IErrorItem AddError(string text, Action<IErrorItem> handler);

        /// <summary>
        /// Adds a warning to the error list.
        /// </summary>
        IErrorItem AddWarning(string text, Action<IErrorItem> handler);

        /// <summary>
        /// Clear the errors.
        /// </summary>
        void ClearErrors();

        /// <summary>
        /// Shows the errors.
        /// </summary>
        void ShowErrors();
    }
}
