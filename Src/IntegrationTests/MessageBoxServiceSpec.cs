using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Clide
{
	[TestClass]
	public class MessageBoxServiceSpec : VsHostedSpec
	{
		[HostType("VS IDE")]
		[Ignore]
		[TestMethod]
		public void WhenShowingMessageBox_ThenCanInvokeShowAndPrompt()
		{
			var service = Container.GetExportedValueOrDefault<IMessageBoxService>();

			service.Show("Hello");

			service.Prompt("Go next?");
		}

        [HostType("VS IDE")]
        [Ignore]
        [TestMethod]
        public void WhenShowingMessageBox_ThenSucceeds()
        {
            var service = Container.GetExportedValueOrDefault<IMessageBoxService>();

            service.Show("Default should be Cancel", button: System.Windows.MessageBoxButton.OKCancel, defaultResult: System.Windows.MessageBoxResult.Cancel);
            service.Show("Default should be OK", button: System.Windows.MessageBoxButton.OKCancel);
            service.Show("Default should be OK", button: System.Windows.MessageBoxButton.OKCancel, defaultResult: System.Windows.MessageBoxResult.OK);
            service.Show("Default should be Yes", button: System.Windows.MessageBoxButton.YesNo, defaultResult: System.Windows.MessageBoxResult.Yes);
            service.Show("Default should be No", button: System.Windows.MessageBoxButton.YesNo, defaultResult: System.Windows.MessageBoxResult.No);
            service.Show("Default should be No", button: System.Windows.MessageBoxButton.YesNoCancel, defaultResult: System.Windows.MessageBoxResult.No);
            service.Show("Default should be Cancel", button: System.Windows.MessageBoxButton.YesNoCancel, defaultResult: System.Windows.MessageBoxResult.Cancel);
            service.Show("Default should be Yes", button: System.Windows.MessageBoxButton.YesNoCancel);

            service.Show("Icon should be Asterisk", icon: System.Windows.MessageBoxImage.Asterisk);
            service.Show("Icon should be Error", icon: System.Windows.MessageBoxImage.Error);
            service.Show("Icon should be Exclamation", icon: System.Windows.MessageBoxImage.Exclamation);
            service.Show("Icon should be Information", icon: System.Windows.MessageBoxImage.Information);
            service.Show("Icon should be Warning", icon: System.Windows.MessageBoxImage.Warning);
            service.Show("Icon should be Stop", icon: System.Windows.MessageBoxImage.Stop);
            service.Show("Icon should be Question", icon: System.Windows.MessageBoxImage.Question);
        }
	}
}
