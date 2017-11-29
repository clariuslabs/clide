namespace Clide
{
	using Microsoft.VisualStudio;
	using Microsoft.VisualStudio.Shell.Interop;
	using System;
	using System.Windows;
	using System.Windows.Interop;
	using System.ComponentModel.Composition;
	using Merq;

	/// <summary>
	/// Implements dialog creation in Visual Studio.
	/// </summary>
	[Export(typeof(IDialogWindowFactory))]
	[PartCreationPolicy(CreationPolicy.Shared)]
	class DialogWindowFactory : IDialogWindowFactory
	{
		readonly Lazy<IVsUIShell> uiShell;
		readonly Lazy<IAsyncManager> asyncManager;

		[ImportingConstructor]
		public DialogWindowFactory(
			[Import(ContractNames.Interop.IVsUIShell)] Lazy<IVsUIShell> uiShell,
			Lazy<IAsyncManager> asyncManager)
		{
			this.uiShell = uiShell;
			this.asyncManager = asyncManager;
		}

		public TView CreateDialog<TView>() where TView : IDialogWindow, new()
		{
			return asyncManager.Value.Run(async () =>
			{
				await asyncManager.Value.SwitchToMainThread();

				return CreateDialogImpl<TView>();
			});
		}

		TView CreateDialogImpl<TView>() where TView : IDialogWindow, new()
		{
			var dialog = new TView();
			var dialogWindow = dialog as Window;
			if (dialogWindow != null)
			{
				IntPtr owner;
				ErrorHandler.ThrowOnFailure(uiShell.Value.GetDialogOwnerHwnd(out owner));
				new WindowInteropHelper(dialogWindow).Owner = owner;
				dialogWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
				dialogWindow.ShowInTaskbar = false;
			}

			return dialog;
		}
	}
}