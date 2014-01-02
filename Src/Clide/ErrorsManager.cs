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
    using Clide.CommonComposition;
    using Microsoft.VisualStudio.Shell;
    using System;
    
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