using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Composition;
namespace Clide
{

    [Export(typeof(IErrorsManager))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class ErrorsManager : IErrorsManager
    {
        readonly IServiceProvider serviceProvider;
        readonly ErrorListProvider errorListProvider;

        [ImportingConstructor]
        public ErrorsManager([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            errorListProvider = new ErrorListProvider(this.serviceProvider);
        }

        public IErrorItem AddError(string text, Action<IErrorItem> handler)
        {
            return Add(text, handler, false);
        }

        public IErrorItem AddWarning(string text, Action<IErrorItem> handler)
        {
            return Add(text, handler, true);
        }

        public void ClearErrors()
        {
            errorListProvider.Tasks.Clear();
        }

        public void ShowErrors()
        {
            errorListProvider.Show();
        }

        private IErrorItem Add(string text, Action<IErrorItem> handler, bool isWarning)
        {
            var errorTask = new ErrorTask();

            errorTask.Category = TaskCategory.Misc;
            errorTask.ErrorCategory = isWarning ? TaskErrorCategory.Warning : TaskErrorCategory.Error;
            errorTask.Text = text;
            errorTask.Document = " ";

            var errorItem = new ErrorItem(errorListProvider, errorTask);

            errorTask.Navigate += (sender, e) =>
            {
                if (handler != null)
                    handler(errorItem);
            };

            errorListProvider.Tasks.Add(errorTask);

            return errorItem;
        }
    }
}
