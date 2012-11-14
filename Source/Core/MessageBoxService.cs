using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using Clide.Properties;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Reflection;
using System.ComponentModel;
using System.Windows;
using Microsoft.VisualStudio;
using System.Diagnostics;
using System.ComponentModel.Composition;

namespace Clide
{
    /// <summary>
    /// Default implementation of the <see cref="IMessageBoxService"/>.
    /// </summary>
    [Export(typeof(IMessageBoxService))]
    internal class MessageBoxService : IMessageBoxService
	{
		private static readonly ITracer tracer = Tracer.Get<MessageBoxService>();
		private IVsUIShell uiShell;
		private IUIThread uiThread;

		/// <summary>
		/// Default constructor for runtime behavior that can't be mocked.
		/// </summary>
		[ImportingConstructor]
		public MessageBoxService(
			[Import(VsContractNames.IVsUIShell)] IVsUIShell uiShell,
			IUIThread uiThread)
		{
			Guard.NotNull(() => uiShell, uiShell);
			Guard.NotNull(() => uiThread, uiThread);

			this.uiShell = uiShell;
			this.uiThread = uiThread;
		}

		public void Show(string message,
			string title = "Visual Studio",
			MessageBoxButton button = MessageBoxButton.OK,
			MessageBoxImage icon = MessageBoxImage.None,
			MessageBoxResult defaultResult = MessageBoxResult.OK)
		{			
			this.uiThread.Invoke(() =>
				MessageBox.Show(this.uiShell.GetMainWindow(), message, title, button, icon, defaultResult));
		}

		public MessageBoxResult Prompt(string message,
			string title = "Visual Studio",
			MessageBoxButton button = MessageBoxButton.OKCancel,
			MessageBoxImage icon = MessageBoxImage.Question,
			MessageBoxResult defaultResult = MessageBoxResult.OK)
		{
			return this.uiThread.Invoke(() =>
				MessageBox.Show(this.uiShell.GetMainWindow(), message, title, button, icon, defaultResult));
		}


		public string InputBox(string message, string title = "Visual Studio")
		{
			var dialog = new InputBox();
			dialog.Message = message;
			dialog.Title = title;
			dialog.ShowInTaskbar = false;
			if (dialog.ShowDialog() == true)
				return dialog.ResponseText;

			return null;
		}
	}
}
