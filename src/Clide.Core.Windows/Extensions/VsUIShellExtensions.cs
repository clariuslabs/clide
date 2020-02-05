using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Windows;
using System.Windows.Interop;
namespace Clide
{

    /// <summary>
    /// Defines extension methods related to <see cref="IVsUIShell"/>.
    /// </summary>
    public static class VsUIShellExtensions
    {
        /// <summary>
        /// Gets the Visual Studio main window.
        /// </summary>
        public static Window GetMainWindow(this IVsUIShell shell)
        {
            IntPtr hwnd;
            ErrorHandler.ThrowOnFailure(shell.GetDialogOwnerHwnd(out hwnd));
            return HwndSource.FromHwnd(hwnd).RootVisual as Window;
        }

        /// <summary>
        /// Shows a message to the user.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the user clicked on Yes/OK, 
        /// <see langword="false"/> if the user clicked No, 
        /// <see langword="null"/> if the user cancelled the dialog or clicked 
        /// <c>Cancel</c> or any other value other than the Yes/OK/No.
        /// </returns>
        public static bool? ShowMessageBox(this IVsUIShell shell, string message,
            string title = MessageBoxServiceDefaults.DefaultTitle,
            MessageBoxButton button = MessageBoxServiceDefaults.DefaultButton,
            MessageBoxImage icon = MessageBoxServiceDefaults.DefaultIcon,
            MessageBoxResult defaultResult = MessageBoxServiceDefaults.DefaultResult)
        {
            var classId = Guid.Empty;
            var result = 0;

            shell.ShowMessageBox(0, ref classId, title, message, string.Empty, 0,
                ToOleButton(button),
                ToOleDefault(defaultResult, button),
                ToOleIcon(icon),
                0, out result);

            if (result == OleMessageBoxResult.IDOK || result == OleMessageBoxResult.IDYES)
                return true;
            else if (result == OleMessageBoxResult.IDNO)
                return false;

            return null;
        }

        /// <summary>
        /// Prompts the user for a response.
        /// </summary>
        public static MessageBoxResult Prompt(this IVsUIShell shell, string message,
            string title = MessageBoxServiceDefaults.DefaultTitle,
            MessageBoxButton button = MessageBoxServiceDefaults.DefaultButton,
            MessageBoxImage icon = MessageBoxImage.Question,
            MessageBoxResult defaultResult = MessageBoxServiceDefaults.DefaultResult)
        {
            var classId = Guid.Empty;
            var result = 0;

            shell.ShowMessageBox(0, ref classId, title, message, string.Empty, 0,
                ToOleButton(button),
                ToOleDefault(defaultResult, button),
                ToOleIcon(icon),
                0, out result);

            return FromOle(result);
        }

        private static OLEMSGBUTTON ToOleButton(MessageBoxButton button)
        {
            switch (button)
            {
                case MessageBoxButton.OKCancel:
                    return OLEMSGBUTTON.OLEMSGBUTTON_OKCANCEL;

                case MessageBoxButton.YesNo:
                    return OLEMSGBUTTON.OLEMSGBUTTON_YESNO;

                case MessageBoxButton.YesNoCancel:
                    return OLEMSGBUTTON.OLEMSGBUTTON_YESNOCANCEL;

                default:
                    return OLEMSGBUTTON.OLEMSGBUTTON_OK;
            }
        }

        private static OLEMSGDEFBUTTON ToOleDefault(MessageBoxResult defaultResult, MessageBoxButton button)
        {
            switch (button)
            {
                case MessageBoxButton.OK:
                    return OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST;

                case MessageBoxButton.OKCancel:
                    if (defaultResult == MessageBoxResult.Cancel)
                        return OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND;

                    return OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST;

                case MessageBoxButton.YesNoCancel:
                    if (defaultResult == MessageBoxResult.No)
                        return OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND;
                    if (defaultResult == MessageBoxResult.Cancel)
                        return OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_THIRD;

                    return OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST;

                case MessageBoxButton.YesNo:
                    if (defaultResult == MessageBoxResult.No)
                        return OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND;

                    return OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST;
            }

            return OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST;
        }

        private static OLEMSGICON ToOleIcon(MessageBoxImage icon)
        {
            switch (icon)
            {
                case MessageBoxImage.Asterisk:
                    return OLEMSGICON.OLEMSGICON_INFO;

                case MessageBoxImage.Error:
                    return OLEMSGICON.OLEMSGICON_CRITICAL;

                case MessageBoxImage.Exclamation:
                    return OLEMSGICON.OLEMSGICON_WARNING;

                case MessageBoxImage.Question:
                    return OLEMSGICON.OLEMSGICON_QUERY;

                default:
                    return OLEMSGICON.OLEMSGICON_NOICON;
            }
        }

        private static MessageBoxResult FromOle(int value)
        {
            switch (value)
            {
                case 1:
                    return MessageBoxResult.OK;

                case 2:
                    return MessageBoxResult.Cancel;

                case 6:
                    return MessageBoxResult.Yes;

                case 7:
                    return MessageBoxResult.No;
            }

            return MessageBoxResult.No;
        }

        /// <summary>
        /// See https://msdn.microsoft.com/en-us/library/microsoft.visualstudio.shell.interop.ivsuishell.showmessagebox.aspx
        /// </summary>
        internal static class OleMessageBoxResult
        {
            public const int IDABORT = 3;
            public const int IDCANCEL = 2;
            public const int IDCLOSE = 8;
            public const int IDCONTINUE = 11;
            public const int IDHELP = 9;
            public const int IDIGNORE = 5;
            public const int IDNO = 7;
            public const int IDOK = 1;
            public const int IDRETRY = 4;
            public const int IDTRYAGAIN = 10;
            public const int IDYES = 6;
        }
    }
}
