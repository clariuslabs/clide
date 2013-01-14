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
    using System;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Clide.Events;
    using Clide.Patterns.Adapter;
    using Moq;
    using Microsoft.VisualStudio;

    public class SolutionNodeSpec
	{
		internal static readonly IAssertion Assert = new Assertion();

		[TestClass]
		public class GivenNoContext : VsHostedSpec
		{
			[HostType("VS IDE")]
			[TestMethod]
			public void WhenCreatingSolution_ThenIsOpenReturnsTrue()
			{
				var solution = ServiceProvider.GetService<IVsSolution>();
				var hierarchy = solution as IVsHierarchy;

				var solutionNode = new SolutionNode(
					new VsSolutionHierarchyNode(hierarchy, VSConstants.VSITEMID_ROOT),
                    Mock.Of<ITreeNodeFactory<IVsSolutionHierarchyNode>>(),
                    Mock.Of<ISolutionExplorerNodeFactory>(),
                    Mock.Of<IServiceProvider>(),
                    Mock.Of<IAdapterService>(),
					Mock.Of<ISolutionEvents>());

				solutionNode.Create(GetFullPath("foo.sln"));

				Assert.True(solutionNode.IsOpen);
			}

			[HostType("VS IDE")]
			[TestMethod]
			public void WhenCreatingSolutionWithInvalidName_ThenThrows()
			{
				var solution = ServiceProvider.GetService<IVsSolution>();
				var hierarchy = solution as IVsHierarchy;

				var solutionNode = new SolutionNode(
                    new VsSolutionHierarchyNode(hierarchy, VSConstants.VSITEMID_ROOT),
					Mock.Of<ITreeNodeFactory<IVsSolutionHierarchyNode>>(),
                    Mock.Of<ISolutionExplorerNodeFactory>(),
                    Mock.Of<IServiceProvider>(),
                    Mock.Of<IAdapterService>(),
					Mock.Of<ISolutionEvents>());

				Assert.Throws<ArgumentException>(() => solutionNode.Create("foo"));
			}

			[HostType("VS IDE")]
			[TestMethod]
			public void WhenSolutionIsOpened_ThenIsOpenReturnsTrue()
			{
				var solution = ServiceProvider.GetService<IVsSolution>();
				var hierarchy = solution as IVsHierarchy;

				var solutionNode = new SolutionNode(
                    new VsSolutionHierarchyNode(hierarchy, VSConstants.VSITEMID_ROOT),
                    Mock.Of<ITreeNodeFactory<IVsSolutionHierarchyNode>>(),
                    Mock.Of<ISolutionExplorerNodeFactory>(),
                    Mock.Of<IServiceProvider>(),
                    Mock.Of<IAdapterService>(),
					Mock.Of<ISolutionEvents>());

				solutionNode.Open(GetFullPath("SampleSolution\\SampleSolution.sln"));

				Assert.True(solutionNode.IsOpen);
			}

            [TestMethod]
            public void WhenGettingParent_ThenReturnsNull()
            {
                var solution = ServiceProvider.GetService<IVsSolution>();
                var hierarchy = solution as IVsHierarchy;

                var solutionNode = new SolutionNode(
                    new VsSolutionHierarchyNode(hierarchy, VSConstants.VSITEMID_ROOT),
                    Mock.Of<ITreeNodeFactory<IVsSolutionHierarchyNode>>(),
                    Mock.Of<ISolutionExplorerNodeFactory>(),
                    Mock.Of<IServiceProvider>(),
                    Mock.Of<IAdapterService>(),
                    Mock.Of<ISolutionEvents>());

                Assert.Null(solutionNode.Parent);
            }
		}
	}
}