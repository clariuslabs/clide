#region BSD License
/* 
Copyright (c) 2012, Clarius Consulting
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

namespace Clide
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel.Composition;
    using System.Windows.Controls;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Forms.Integration;
    using System.Windows.Interop;
    using System.Windows.Input;
    using System.ComponentModel;
    using Clide.Composition;

    [Component(typeof(IOptionsPageWindowFactory))]
	internal class OptionsPageWindowFactory : IOptionsPageWindowFactory
	{
        private IMessageBoxService messageBox;

        public OptionsPageWindowFactory(IMessageBoxService messageBox)
        {
            this.messageBox = messageBox;
        }

		public System.Windows.Forms.IWin32Window CreateWindow(IEditableObject model, UserControl view)
		{
			return new ToolsOptionsPageWin32Window(model, view, this.messageBox);
		}

		private class EditableDialogPage
		{
			private IEditableObject model;
            private IMessageBoxService messageBox;

			public EditableDialogPage(IEditableObject model, IMessageBoxService messageBox)
			{
				this.model = model;
                this.messageBox = messageBox;
			}

			internal void OnActivate(CancelEventArgs args)
			{
				this.model.BeginEdit();
			}

			internal void OnApply(PageApplyEventArgs args)
			{
				if (args.ApplyBehavior == ApplyKind.Apply)
				{
                    try
                    {
                        this.model.EndEdit();
                    }
                    catch (Exception ex)
                    {
                        this.messageBox.Show(ex.Message, icon: MessageBoxImage.Error);
                        args.ApplyBehavior = ApplyKind.CancelNoNavigate;
                    }
                }
				else
				{
					this.model.CancelEdit();
				}
			}

			internal void OnDeactivate(CancelEventArgs ce)
			{
			}

			internal void OnClosed(EventArgs eventArgs)
			{
				this.model.CancelEdit();
			}
		}

		private enum ApplyKind
		{
			Apply,
			Cancel,
			CancelNoNavigate
		}

		private class PageApplyEventArgs : EventArgs
		{
			public ApplyKind ApplyBehavior { get; set; }
		}

		private class ToolsOptionsPageWin32Window : System.Windows.Forms.IWin32Window
		{
			private UserControl view;
			private DialogPageElementHost elementHost;
			private IEditableObject model;
			private DialogSubclass subClass;
            private IMessageBoxService messageBox;

			public ToolsOptionsPageWin32Window(IEditableObject model, UserControl view, IMessageBoxService messageBox)
			{
				this.model = model;
				this.view = view;
                this.messageBox = messageBox;

				this.elementHost = new DialogPageElementHost();
				this.elementHost.Dock = System.Windows.Forms.DockStyle.Fill;

				// Hooks the child hwnd source.
				PresentationSource.AddSourceChangedHandler(this.view, this.OnSourceChanged);

				this.elementHost.Child = this.view;
			}

			public IntPtr Handle
			{
				get
				{
					if (this.subClass == null)
					{
						this.subClass = new DialogSubclass(new EditableDialogPage(this.model, this.messageBox));
					}
					if (this.subClass.Handle != this.elementHost.Handle)
					{
						this.subClass.AssignHandle(this.elementHost.Handle);
					}

					return this.elementHost.Handle;
				}
			}

			private void OnSourceChanged(object sender, SourceChangedEventArgs e)
			{
				var oldSource = e.OldSource as HwndSource;
				var newSource = e.NewSource as HwndSource;

				if (oldSource != null)
					oldSource.RemoveHook(new HwndSourceHook(this.SourceHook));

				if (newSource != null)
					newSource.AddHook(new HwndSourceHook(this.SourceHook));
			}

			private IntPtr SourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
			{
				if (msg == 0x87)
				{
					handled = true;
					return new IntPtr(0x83);
				}

				return IntPtr.Zero;
			}

			/// <devdoc>
			///     This class derives from NativeWindow to provide a hook
			///     into the window handle.  We use this hook so we can
			///     respond to property sheet window messages that VS
			///     will send us.
			/// </devdoc>
			private sealed class DialogSubclass : System.Windows.Forms.NativeWindow
			{
				private EditableDialogPage page;
				private bool closeCalled;

				/// <devdoc>
				///     Create a new DialogSubclass
				/// </devdoc>
				internal DialogSubclass(EditableDialogPage page)
				{
					this.page = page;
					this.closeCalled = false;
				}

				/// <devdoc>
				///     Override for WndProc to handle our PSP messages
				/// </devdoc>
				protected override void WndProc(ref System.Windows.Forms.Message m)
				{
					CancelEventArgs ce;

					switch (m.Msg)
					{
						case NativeMethods.WM_NOTIFY:
							NativeMethods.NMHDR nmhdr = (NativeMethods.NMHDR)Marshal.PtrToStructure(m.LParam, typeof(NativeMethods.NMHDR));
							switch (nmhdr.code)
							{
								case NativeMethods.PSN_RESET:
									closeCalled = true;
									page.OnClosed(EventArgs.Empty);
									return;
								case NativeMethods.PSN_APPLY:
									PageApplyEventArgs pae = new PageApplyEventArgs();
									page.OnApply(pae);
									switch (pae.ApplyBehavior)
									{
										case ApplyKind.Cancel:
											m.Result = (IntPtr)NativeMethods.PSNRET_INVALID;
											break;

										case ApplyKind.CancelNoNavigate:
											m.Result = (IntPtr)NativeMethods.PSNRET_INVALID_NOCHANGEPAGE;
											break;

										case ApplyKind.Apply:
										default:
											m.Result = IntPtr.Zero;
											break;
									}
									UnsafeNativeMethods.SetWindowLong(m.HWnd, NativeMethods.DWL_MSGRESULT, m.Result);
									return;
								case NativeMethods.PSN_KILLACTIVE:
									ce = new CancelEventArgs();
									page.OnDeactivate(ce);
									m.Result = (IntPtr)(ce.Cancel ? 1 : 0);
									UnsafeNativeMethods.SetWindowLong(m.HWnd, NativeMethods.DWL_MSGRESULT, m.Result);
									return;
								case NativeMethods.PSN_SETACTIVE:
									closeCalled = false;
									ce = new CancelEventArgs();
									page.OnActivate(ce);
									m.Result = (IntPtr)(ce.Cancel ? -1 : 0);
									UnsafeNativeMethods.SetWindowLong(m.HWnd, NativeMethods.DWL_MSGRESULT, m.Result);
									return;
							}
							break;
						case NativeMethods.WM_DESTROY:
							// we can't tell the difference between OK and Apply (see above), so
							// if we get a destroy and close hasn't been called, make sure we call it
							//
							if (!closeCalled && page != null)
							{
								page.OnClosed(EventArgs.Empty);
							}
							break;
					}

					base.WndProc(ref m);
				}
			}

			private class DialogKeyboardInputSite : IKeyboardInputSite
			{
				private HwndSource source;

				public DialogKeyboardInputSite(HwndSource source)
				{
					this.source = source;
				}

				public bool OnNoMoreTabStops(TraversalRequest request)
				{
					var flag = true;
					if (request != null)
					{
						switch (request.FocusNavigationDirection)
						{
							case FocusNavigationDirection.Next:
							case FocusNavigationDirection.Right:
							case FocusNavigationDirection.Down:
								flag = true;
								break;
							case FocusNavigationDirection.Previous:
							case FocusNavigationDirection.Left:
							case FocusNavigationDirection.Up:
								flag = false;
								break;
						}
					}

					var ancestor = NativeMethods.GetAncestor(this.source.Handle, 2);
					if (ancestor != IntPtr.Zero)
					{
						var hWnd = NativeMethods.GetNextDlgTabItem(ancestor, this.source.Handle, !flag);
						if (hWnd != IntPtr.Zero)
						{
							NativeMethods.SetFocus(hWnd);
							return true;
						}
					}

					return false;
				}

				public void Unregister() { }

				public IKeyboardInputSink Sink { get { return this.source; } }
			}

			private class DialogPageElementHost : ElementHost
			{
				protected override void OnGotFocus(EventArgs e)
				{
					base.OnGotFocus(e);
					var source = PresentationSource.FromVisual(base.Child) as HwndSource;
					UIElement element = null;
					if ((source != null) && ((element = source.RootVisual as UIElement) != null))
					{
						element.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
					}
				}

				protected override void OnHandleCreated(EventArgs e)
				{
					base.OnHandleCreated(e);
					var source = PresentationSource.FromVisual(base.Child) as HwndSource;
					if (source != null)
					{
						((IKeyboardInputSink)source).KeyboardInputSite = new DialogKeyboardInputSite(source);
					}
				}
			}

			private static class NativeMethods
			{
				public const int WM_NOTIFY = 0x4e;
				public const int WM_DESTROY = 2;
				public const int DWL_MSGRESULT = 0;
				public const int PSN_SETACTIVE = -200;
				public const int PSN_KILLACTIVE = -201;
				public const int PSN_APPLY = -202;
				public const int PSN_RESET = -203;
				public const int PSNRET_INVALID = 1;
				public const int PSNRET_INVALID_NOCHANGEPAGE = 2;

				[DllImport("user32.dll", ExactSpelling = true)]
				public static extern IntPtr SetFocus(IntPtr hWnd);

				[DllImport("user32.dll", ExactSpelling = true)]
				public static extern IntPtr GetAncestor(IntPtr hWnd, int flags);

				[DllImport("user32.dll", ExactSpelling = true)]
				public static extern IntPtr GetNextDlgTabItem(IntPtr hDlg, IntPtr hCtl, [MarshalAs(UnmanagedType.Bool)] bool bPrevious);

				[StructLayout(LayoutKind.Sequential)]
				public struct NMHDR
				{
					public IntPtr hwndFrom;
					public int idFrom;
					public int code;
				}
			}

			private static class UnsafeNativeMethods
			{
				[DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto)]
				public static extern IntPtr SetWindowLongPtr32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

				[DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", CharSet = CharSet.Auto)]
				public static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

				public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
				{
					if (IntPtr.Size == 4)
					{
						return SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
					}
					return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
				}
			}
		}
	}
}