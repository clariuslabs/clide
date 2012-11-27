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
using System;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide
{
	internal class StatusBar : IStatusBar
	{
		private IServiceProvider serviceProvider;
		private Lazy<IVsStatusbar> bar;

		public StatusBar(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
			this.bar = new Lazy<IVsStatusbar>(() => this.serviceProvider.GetService<SVsStatusbar, IVsStatusbar>());
		}

		public void Clear()
		{
			this.bar.Value.Clear();
		}

		public void ShowMessage(string message)
		{
			int frozen;

			this.bar.Value.IsFrozen(out frozen);

			if (frozen == 0)
			{
				this.bar.Value.SetText(message);
			}
		}

		public void ShowProgress(string message, int complete, int total)
		{
			int frozen;

			this.bar.Value.IsFrozen(out frozen);

			if (frozen == 0)
			{
				uint cookie = 0;

				if (complete != total)
				{
					this.bar.Value.Progress(ref cookie, 1, message, (uint)complete, (uint)total);
				}
				else
				{
					this.bar.Value.Progress(ref cookie, 0, string.Empty, (uint)complete, (uint)total);
				}
			}
		}
	}
}