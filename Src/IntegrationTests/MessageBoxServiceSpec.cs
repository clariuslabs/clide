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

namespace Clide
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
	public class MessageBoxServiceSpec : VsHostedSpec
	{
		[HostType("VS IDE")]
		[Ignore]
		[TestMethod]
		public void WhenShowingMessageBox_ThenCanInvokeShowAndPrompt()
		{
			var service = ServiceLocator.GetInstance<IMessageBoxService>();

			service.Show("Hello");

			service.Prompt("Go next?");
		}

        [HostType("VS IDE")]
        [Ignore]
        [TestMethod]
        public void WhenShowingMessageBox_ThenSucceeds()
        {
            var service = ServiceLocator.GetInstance<IMessageBoxService>();

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
