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
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Windows.Controls;

    /// <summary>
    /// Base class for options pages.
    /// </summary>
    /// <typeparam name="TControl">The type of the control implementing the user interface.</typeparam>
    /// <typeparam name="TSettings">The type of the settings that persist the values of the page.</typeparam>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsPage{TControl, TSettings}"/> class.
        /// </summary>
        /// <param name="windowFactory">The window factory.</param>
        /// <param name="settings">The settings.</param>
		protected OptionsPage(IOptionsPageWindowFactory windowFactory, TSettings settings)
		{
            this.windowFactory = windowFactory;
            this.settings = settings;
			this.userControl = new Lazy<UserControl>(() =>
				new TControl { DataContext = this.settings });

			this.windowHandle = new Lazy<System.Windows.Forms.IWin32Window>(() =>
				this.windowFactory.CreateWindow(this.settings, this.userControl.Value));
		}

        /// <summary>
        /// Gets the handle to the window represented by the implementer.
        /// </summary>
        /// <returns>A handle to the window represented by the implementer.</returns>
		public IntPtr Handle
		{
			get { return this.windowHandle.Value.Handle; }
		}
	}
}
