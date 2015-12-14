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
	using Microsoft.VisualStudio;
	using Microsoft.VisualStudio.Shell;
	using Microsoft.VisualStudio.Shell.Interop;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Microsoft.VSSDK.Tools.VsIdeTesting;
	using System;
	using System.Collections.Generic;

    [TestClass]
    public class Misc : VsHostedSpec
    {
		[HostType("VS IDE")]
		[TestMethod]
		public void when_Action_then_Assert()
		{
			Console.WriteLine("RegType_Configuration: " + VSRegistry.RegistryRoot(__VsLocalRegistryType.RegType_Configuration).Name);
			Console.WriteLine("RegType_UserSettings: " + VSRegistry.RegistryRoot(__VsLocalRegistryType.RegType_UserSettings).Name);

			using (var vsRoot = VSRegistry.RegistryRoot (__VsLocalRegistryType.RegType_UserSettings)) {
				var registryRoot = vsRoot.Name.Substring(vsRoot.Name.IndexOf('\\') + 1);
				Console.WriteLine (registryRoot);
			}

			// /// <summary>The build version of the release and the branch, machine, and user information used to build it (for example, "10.0.30319.01 RTMRel" or "10.0.30128.1 BRANCHNAME(COMPUTERNAME-USERNAME)"). This is the same as the release string shown in Help/About.</summary>
			var VSSPROPID_ReleaseVersion = -9068;
			WriteProperty ("VSSPROPID_ReleaseVersion", VSSPROPID_ReleaseVersion);

			// /// <summary>The branding for this release (for example, CTP, Beta, RTM, and so on). This property is read only.</summary>
			var VSSPROPID_ReleaseDescription = -9069;
			WriteProperty ("VSSPROPID_ReleaseDescription", VSSPROPID_ReleaseDescription);

			var VSSPROPID_SKUInfo = -9073;
			WriteProperty ("VSSPROPID_SKUInfo", VSSPROPID_SKUInfo);

			WriteProperty ("VSSPROPID_AppBrandName", -9071);
			WriteProperty ("VSSPROPID_AppShortBrandName", -9072);
		}

		private void WriteProperty (string propName, int propId)
		{
			var shell = base.ServiceProvider.GetService<SVsShell, IVsShell> ();
			object prop;
			shell.GetProperty (propId, out prop);
			Console.WriteLine ("{0}: {1}", propName, prop);
		}

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenRetrievingHierarchyItem_ThenAlwaysGetsSameInstance()
        {
            this.OpenSolution("SampleSolution\\SampleSolution.sln");
            var projectId = new Guid("{883173CE-9FDE-4436-96F5-128EA8A471BE}");

            var solution = this.ServiceProvider.GetService<SVsSolution, IVsSolution>();

            IVsHierarchy hierarchy1;
            IVsHierarchy hierarchy2;

            ErrorHandler.ThrowOnFailure(solution.GetProjectOfGuid(ref projectId, out hierarchy1));
            ErrorHandler.ThrowOnFailure(solution.GetProjectOfGuid(ref projectId, out hierarchy2));

            Assert.IsNotNull(hierarchy1);
            Assert.IsNotNull(hierarchy2);

            Assert.AreSame(hierarchy1, hierarchy2);

            var hashCodes = new List<int>(new[] { hierarchy1.GetHashCode(), hierarchy2.GetHashCode() });

            this.CloseSolution();

            this.OpenSolution("SampleSolution\\SampleSolution.sln");

            solution = this.ServiceProvider.GetService<SVsSolution, IVsSolution>();

            ErrorHandler.ThrowOnFailure(solution.GetProjectOfGuid(ref projectId, out hierarchy2));

            Assert.AreNotSame(hierarchy1, hierarchy2);

            Assert.IsFalse(hashCodes.Contains(hierarchy2.GetHashCode()));
        }

        // This test only passes on VS2012, meaning they changed the behavior 
        // of the IVsSolution service...
        [Ignore]
        [HostType("VS IDE")]
        [TestMethod]
        public void WhenClosingAndReopeningSolution_ThenSolutionInstanceIsDifferent()
        {
            this.OpenSolution("SampleSolution\\SampleSolution.sln");
            var solution = this.ServiceProvider.GetService<SVsSolution, IVsSolution>();
            var solutionHashCode = solution.GetHashCode();

            this.CloseSolution();
            this.OpenSolution("SampleSolution\\SampleSolution.sln");

            solution = this.ServiceProvider.GetService<SVsSolution, IVsSolution>();

            Assert.AreNotEqual(solutionHashCode, solution.GetHashCode());
        }
    }
}
