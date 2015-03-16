#region BSD License
/* 
Copyright (c) 2012, Clarius Consulting
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

namespace Clide
{
	using Microsoft.VisualStudio;
	using Microsoft.VisualStudio.Shell.Interop;
	using System;
	using System.Windows;
	using System.Windows.Interop;

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
			string title = MessageBoxService.DefaultTitle,
			MessageBoxButton button = MessageBoxService.DefaultButton,
			MessageBoxImage icon = MessageBoxService.DefaultIcon,
			MessageBoxResult defaultResult = MessageBoxService.DefaultResult)
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
			string title = MessageBoxService.DefaultTitle,
			MessageBoxButton button = MessageBoxService.DefaultButton,
			MessageBoxImage icon = MessageBoxImage.Question,
			MessageBoxResult defaultResult = MessageBoxService.DefaultResult)
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
		/// See https://msdn.microsoft.com/en-us/library/microsoft.visualstudio.shell.interop.ivsuishell.showmessagebox.aspx?f=255&MSPPError=-2147217396
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