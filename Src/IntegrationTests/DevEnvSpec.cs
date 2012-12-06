using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace Clide
{
	[TestClass]
	public class DevEnvSpec : VsHostedSpec
	{
		internal static readonly IAssertion Assert = new Assertion();

		[HostType("VS IDE")]
		[TestMethod]
		public void WhenEnvironmentInitialized_ThenRaisesInitializedEvent()
		{
            var devenv = Container.GetExportedValue<IDevEnv>();

			var called = false;

			devenv.Initialized += (sender, args) => called = true;

			var maxWait = DateTime.Now.AddSeconds(5);
			while (!devenv.IsInitialized && DateTime.Now < maxWait)
			{
				Thread.Sleep(50);
			}

			Assert.True(devenv.IsInitialized);
			Assert.True(called);
		}
	}
}
