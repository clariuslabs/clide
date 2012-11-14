using System;
using Microsoft.VisualStudio.Shell.Interop;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;

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