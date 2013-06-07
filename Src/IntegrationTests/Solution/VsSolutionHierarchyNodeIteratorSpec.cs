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

namespace Clide.Solution
{
    using Clide.VisualStudio;
    using System;
    using System.Linq;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.VisualStudio;

    public class VsSolutionHierarchyNodeIteratorSpec
	{
		internal static readonly IAssertion Assert = new Assertion();

		[TestClass]
		public class GivenASolution : VsHostedSpec
		{
			[TestInitialize]
			public override void TestInitialize()
			{
				base.TestInitialize();
				base.OpenSolution("SampleSolution\\SampleSolution.sln");
			}

            [TestCleanup]
            public override void TestCleanup()
            {
                base.TestCleanup();
                base.CloseSolution();
            }

			[HostType("VS IDE")]
			[TestMethod]
			public void WhenIteratingTopLevelNodes_ThenGetsTwoFolders()
			{
				var solution = ServiceProvider.GetService<IVsSolution>();
				var hierarchy = solution as IVsHierarchy;

				var nodes = new VsSolutionHierarchyNode(hierarchy, VSConstants.VSITEMID_ROOT).Children.Select(n => 
                    n.VsHierarchy.Properties(n.ItemId).DisplayName).ToList();

				Assert.Equal(2, nodes.Count);
			}

			[HostType("VS IDE")]
			[TestMethod]
			public void WhenIteratingSolutionFolder1_ThenGetsTwoNodes()
			{
				var solution = ServiceProvider.GetService<IVsSolution>();
				var hierarchy = solution as IVsHierarchy;

                var solutionFolder1 = new VsSolutionHierarchyNode(hierarchy, VSConstants.VSITEMID_ROOT).Children
                    .FirstOrDefault(n => n.VsHierarchy.Properties(n.ItemId).DisplayName == "SolutionFolder1");

				Assert.Equal(2, solutionFolder1.Children.Count());
			}

			[HostType("VS IDE")]
			[TestMethod]
			public void WhenIteratingClassLibraryProject_ThenGetsFourNodes()
			{
				var solution = ServiceProvider.GetService<IVsSolution>();
				var hierarchy = solution as IVsHierarchy;

                var solutionFolder1 = new VsSolutionHierarchyNode(hierarchy, VSConstants.VSITEMID_ROOT).Children
                    .FirstOrDefault(n => n.VsHierarchy.Properties(n.ItemId).DisplayName == "SolutionFolder1");
				var solutionFolder2 = solutionFolder1.Children
                    .FirstOrDefault(n => n.VsHierarchy.Properties(n.ItemId).DisplayName == "SolutionFolder2");
				var project = solutionFolder2.Children
                    .FirstOrDefault(n => n.VsHierarchy.Properties(n.ItemId).DisplayName == "ClassLibrary");

                var count = project.Children.Count();
				Assert.Equal(4, count, "Expected 4 child nodes for project, but instead got {0}", count);
			}

			[HostType("VS IDE")]
			[TestMethod]
			public void WhenIteratingClassLibraryProjectFolder_ThenGetsTwoNodes()
			{
				var solution = ServiceProvider.GetService<IVsSolution>();
				var hierarchy = solution as IVsHierarchy;

                var solutionFolder1 = new VsSolutionHierarchyNode(hierarchy, VSConstants.VSITEMID_ROOT).Children.FirstOrDefault(n => n.VsHierarchy.Properties(n.ItemId).DisplayName == "SolutionFolder1");
				var solutionFolder2 = solutionFolder1.Children.FirstOrDefault(n => n.VsHierarchy.Properties(n.ItemId).DisplayName == "SolutionFolder2");
				var project = solutionFolder2.Children.FirstOrDefault(n => n.VsHierarchy.Properties(n.ItemId).DisplayName == "ClassLibrary");

				var folder = project.Children.FirstOrDefault(n => n.VsHierarchy.Properties(n.ItemId).DisplayName == "Folder");

				Assert.Equal(2, folder.Children.Count());
			}

			[HostType("VS IDE")]
			[TestMethod]
			public void WhenIteratingClassLibraryProjectFolderFolder_ThenGetsOneNode()
			{
				var solution = ServiceProvider.GetService<IVsSolution>();
				var hierarchy = solution as IVsHierarchy;

                var solutionFolder1 = new VsSolutionHierarchyNode(hierarchy, VSConstants.VSITEMID_ROOT).Children
                    .FirstOrDefault(n => n.VsHierarchy.Properties(n.ItemId).DisplayName == "SolutionFolder1");
				var solutionFolder2 = solutionFolder1.Children
                    .FirstOrDefault(n => n.VsHierarchy.Properties(n.ItemId).DisplayName == "SolutionFolder2");
				var project = solutionFolder2.Children
                    .FirstOrDefault(n => n.VsHierarchy.Properties(n.ItemId).DisplayName == "ClassLibrary");

				var folder = project.Children.FirstOrDefault(n => n.VsHierarchy.Properties(n.ItemId).DisplayName == "Folder");

				folder = folder.Children.FirstOrDefault(n => n.VsHierarchy.Properties(n.ItemId).DisplayName == "Folder");

				Assert.Equal(1, folder.Children.Count());
			}
		}
	}
}