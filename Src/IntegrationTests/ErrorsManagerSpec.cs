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
    public class ErrorsManagerSpec : VsHostedSpec
	{
		[HostType("VS IDE")]
		[TestMethod]
		public void WhenAddingError_ThenItemIsNotNull()
		{
			var manager = ServiceLocator.GetInstance<IErrorsManager>();
            manager.ClearErrors();

            var item = manager.AddError("Error1", null);

            Assert.IsNotNull(item);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenAddingError_ThenItemIsAddedToTheErrorList()
        {
            var manager = ServiceLocator.GetInstance<IErrorsManager>();
            manager.ClearErrors();

            var item = manager.AddError("Error1", null);

            var dte = this.ServiceProvider.GetService(typeof(EnvDTE.DTE)) as EnvDTE80.DTE2;

            Assert.AreEqual(1, dte.ToolWindows.ErrorList.ErrorItems.Count);
            Assert.AreEqual("Error1", dte.ToolWindows.ErrorList.ErrorItems.Item(1).Description);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenRemovingError_ThenItemIsRemovedFromTheErrorList()
        {
            var manager = ServiceLocator.GetInstance<IErrorsManager>();
            manager.ClearErrors();

            var item = manager.AddError("Error1", null);

            item.Remove();

            var dte = this.ServiceProvider.GetService(typeof(EnvDTE.DTE)) as EnvDTE80.DTE2;

            Assert.AreEqual(0, dte.ToolWindows.ErrorList.ErrorItems.Count);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenClearingErrors_ThenErrorsAreRemovedFromTheErrorList()
        {
            var manager = ServiceLocator.GetInstance<IErrorsManager>();

            var item1 = manager.AddError("Error1", null);
            var item2 = manager.AddError("Error2", null);
            var item3 = manager.AddError("Error3", null);

            manager.ClearErrors();

            var dte = this.ServiceProvider.GetService(typeof(EnvDTE.DTE)) as EnvDTE80.DTE2;

            Assert.AreEqual(0, dte.ToolWindows.ErrorList.ErrorItems.Count);
        }
    }
}