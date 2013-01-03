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
	}
}
