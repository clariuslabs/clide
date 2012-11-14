using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Interop;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.Linq;

namespace Clide
{
	/// <summary>
	/// Implements dialog creation in Visual Studio.
	/// </summary>
	[PartCreationPolicy(CreationPolicy.Shared)]
	[Export(typeof(IDialogWindowFactory))]
	internal class DialogWindowFactory : IDialogWindowFactory
	{
		private static readonly MethodInfo ComposeExportedValueMethod = Reflect<CompositionContainer>.GetMethod(x => x.ComposeExportedValue<string>(null)).GetGenericMethodDefinition();

		private IComponentModel components;
		private IVsUIShell uiShell;
		private IUIThread uiThread;

		[ImportingConstructor]
		public DialogWindowFactory(
			[Import(VsContractNames.IComponentModel)] IComponentModel components,
			[Import(VsContractNames.IVsUIShell)] IVsUIShell uiShell,
			IUIThread uiThread)
		{
			Guard.NotNull(() => components, components);
			Guard.NotNull(() => uiShell, uiShell);
			Guard.NotNull(() => uiThread, uiThread);

			this.components = components;
			this.uiShell = uiShell;
			this.uiThread = uiThread;
		}

		public TView CreateDialog<TView, TDataContext>(params object[] dynamicContextValues)
			where TView : IDialogWindow, new()
			where TDataContext : class
		{
			TView view = default(TView);
			Action action = () =>
			{
				view = CreateDialogImpl<TView>();

				// Optimize code path if no dynamic context was deceived.
				if (dynamicContextValues == null || dynamicContextValues.Length == 0)
				{ 
					 view.DataContext = this.components.GetService<TDataContext>();
				}
				else
				{
					var container = new CompositionContainer(this.components.DefaultExportProvider);
					foreach (var dynamicValue in dynamicContextValues.Where(value => value != null))
					{
						var composeValue = ComposeExportedValueMethod.MakeGenericMethod(dynamicValue.GetType());
						composeValue.Invoke(container, new object[] { dynamicValue });

						foreach (var iface in dynamicValue.GetType().GetInterfaces())
						{
							composeValue = ComposeExportedValueMethod.MakeGenericMethod(iface);
							composeValue.Invoke(container, new object[] { dynamicValue });
						}
					}
				}
			};

			this.uiThread.Invoke(action);

			return view;
		}

		public TView CreateDialog<TView>() where TView : IDialogWindow, new()
		{
			TView view = default(TView);

			Action action = () =>
			{
				view = CreateDialogImpl<TView>();
			};

			this.uiThread.Invoke(action);

			return view;
		}

		private TView CreateDialogImpl<TView>() where TView : IDialogWindow, new()
		{
			var dialog = new TView();
			var dialogWindow = dialog as Window;
			if (dialogWindow != null)
			{
				IntPtr owner;
				ErrorHandler.ThrowOnFailure(this.uiShell.GetDialogOwnerHwnd(out owner));
				new WindowInteropHelper(dialogWindow).Owner = owner;
				dialogWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
				dialogWindow.ShowInTaskbar = false;
				// This would not set the right owner.
				//dialogWindow.Owner = Application.Current.MainWindow;
			}

			return dialog;
		}
	}
}