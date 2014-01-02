using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Clide.CommonComposition;
using Microsoft.VisualStudio.Shell;

namespace Clide
{
	[Component(IsSingleton = true)]
	internal class ErrorsManager : IErrorsManager
	{
		private IServiceProvider serviceProvider;
		private ErrorListProvider errorListProvider;

		public ErrorsManager(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
			this.errorListProvider = new ErrorListProvider(this.serviceProvider);
		}

		public IErrorItem AddError(string text, Action<IErrorItem> handler)
		{
			var errorTask = new ErrorTask();

			errorTask.Category = TaskCategory.Misc;
			errorTask.ErrorCategory = TaskErrorCategory.Error;
			errorTask.Text = text;

			var errorItem = new ErrorItem(this.errorListProvider, errorTask);

			errorTask.Navigate += (sender, e) =>
				{
					handler(errorItem);
				};

			this.errorListProvider.Tasks.Add(errorTask);

			return errorItem;
		}

		public void ClearErrors()
		{
			this.errorListProvider.Tasks.Clear();
		}

		public void ShowErrors()
		{
			this.errorListProvider.Show();
		}
	}
}