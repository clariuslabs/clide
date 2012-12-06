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
    using System;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Interop;
    using Clide.Composition;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.ComponentModelHost;
    using Microsoft.VisualStudio.Shell.Interop;

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