﻿namespace Clide
{

    /// <summary>
    /// Provides usability overloads for the <see cref="IMessageBoxService"/>.
    /// </summary>
    public static class IMessageBoxServiceExtensions
    {
        /// <summary>
        /// Shows an information dialog.
        /// </summary>
        public static void ShowInformation(this IMessageBoxService service, string message)
        {
            service.Show(message, icon: MessageBoxImage.Information);
        }

        /// <summary>
        /// Shows a warning dialog.
        /// </summary>
        public static void ShowWarning(this IMessageBoxService service, string message)
        {
            service.Show(message, icon: MessageBoxImage.Warning);
        }
    }
}