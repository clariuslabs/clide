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
	}
}