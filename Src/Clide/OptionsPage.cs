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
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Controls;
    using System.ComponentModel;
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Shell;

    [DesignerCategory("Code")]
	[ComVisible(true)]
	public abstract class OptionsPage<TControl, TSettings> : Component, IOptionsPage
		where TControl : UserControl, new()
		where TSettings : ISettings
	{
		private TSettings settings;
		private Lazy<UserControl> userControl;
		private Lazy<System.Windows.Forms.IWin32Window> windowHandle;

        private IOptionsPageWindowFactory windowFactory;

		protected OptionsPage(IOptionsPageWindowFactory windowFactory, TSettings settings)
		{
            this.windowFactory = windowFactory;
            this.settings = settings;
			this.userControl = new Lazy<UserControl>(() =>
				new TControl { DataContext = this.settings });

			this.windowHandle = new Lazy<System.Windows.Forms.IWin32Window>(() =>
				this.windowFactory.CreateWindow(this.settings, this.userControl.Value));
		}

		public IntPtr Handle
		{
			get { return this.windowHandle.Value.Handle; }
		}
	}
}
