using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clide
{
	internal class ErrorItem : IErrorItem
	{
		private ErrorListProvider provider;
		private ErrorTask task;

		public ErrorItem(ErrorListProvider provider, ErrorTask task)
		{
			this.provider = provider;
			this.task = task;
		}

		public void Remove()
		{
			if (this.provider.Tasks.Contains(this.task))
				this.provider.Tasks.Remove(this.task);
		}
	}
}
