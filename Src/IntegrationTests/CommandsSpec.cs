using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Shell.Interop;

namespace Clide
{
    [TestClass]
    public class CommandsSpec : VsHostedSpec
    {
        [HostType("VS IDE")]
        [TestMethod]
        public void WhenRegisteringAllCommands_ThenCanRetrieveCommand()
        {
            var package = this.ShellPackage;

            // TODO: add test.
        }
    }
}
