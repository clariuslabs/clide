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
    using Clide.Solution;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Threading;

    [TestClass]
	public class DevEnvSpec : VsHostedSpec
	{
		internal static readonly IAssertion Assert = new Assertion();

        [HostType("VS IDE")]
        [TestMethod]
        public void when_getting_global_devenv_then_succeeds()
        {
            var dev = DevEnv.Get(Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider);

            Assert.NotNull(dev);
        }

		[HostType("VS IDE")]
		[TestMethod]
		public void WhenEnvironmentInitialized_ThenRaisesInitializedEvent()
		{
            var devenv = ServiceLocator.GetInstance<IDevEnv>();

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

        [HostType("VS IDE")]
        [TestMethod]
        [Ignore]
        public void when_quitting_vs_then_ends_process()
        {
            var devenv = ServiceLocator.GetInstance<IDevEnv>();

            devenv.Exit();
        }

        [HostType("VS IDE")]
        [TestMethod]
        [Ignore]
        public void when_quitting_vs_while_building_then_cancels_build_and_ends_process()
        {
            var devenv = ServiceLocator.GetInstance<IDevEnv>();
            OpenSolution("SampleSolution\\SampleSolution.sln");

            devenv.SolutionExplorer().Solution.Build();

            devenv.Exit();
        }

        [HostType("VS IDE")]
        [TestMethod]
        // The test dies before it can catch the exception :)
        // This is because the tested instance of VS just restarted, so it's expected.
        [ExpectedException(typeof(System.Runtime.Remoting.RemotingException))]
        [Ignore]
        public void when_restarting_vs_while_building_then_cancels_build_and_restarts()
        {
            var devenv = ServiceLocator.GetInstance<IDevEnv>();
            OpenSolution("SampleSolution\\SampleSolution.sln");

            devenv.SolutionExplorer().Solution.Build();

            Assert.True(devenv.Restart(true));

            // If we sleep enough here we will see VS starting to come up again.
            Thread.Sleep(50000);
        }
    }
}
