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
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition.Hosting;
    using System.Linq;

    public class SolutionExplorerSpec
	{
		internal static readonly IAssertion Assert = new Assertion();

		[TestClass]
		public class GivenNocontext : VsHostedSpec
		{
            [HostType("VS IDE")]
            [TestMethod]
            public void WhenGettingDefaultHierarchyFactory_ThenSucceeds()
            {
                var factory = base.ServiceLocator.GetInstance<ITreeNodeFactory<IVsSolutionHierarchyNode>>(DefaultHierarchyFactory.RegisterKey);

                Assert.NotNull(factory);

                var nodefactories = base.ServiceLocator.GetAllInstances<Lazy<ITreeNodeFactory<IVsSolutionHierarchyNode>, ITreeNodeFactoryMetadata>>()
                    .ToList();

                foreach (var node in nodefactories)
                {
                    Console.WriteLine("{0} (is fallback: {1}", node.Value, node.Metadata.IsFallback);
                }
            }

			[HostType("VS IDE")]
			[TestMethod]
			public void WhenGettingSolutionExplorer_ThenReturnsInstance()
			{
				var solutionExplorer = base.ServiceLocator.GetInstance<ISolutionExplorer>();

				Assert.NotNull(solutionExplorer);
			}

			[HostType("VS IDE")]
			[TestMethod]
			public void WhenClosingSolutionExplorer_ThenIsOpenReturnsFalse()
			{
                var solutionExplorer = base.ServiceLocator.GetInstance<ISolutionExplorer>();

				solutionExplorer.Close();

				Assert.False(solutionExplorer.IsVisible);
			}

			[HostType("VS IDE")]
			[TestMethod]
			public void WhenOpeningSolutionExplorer_ThenIsOpenReturnsTrue()
			{
                var solutionExplorer = base.ServiceLocator.GetInstance<ISolutionExplorer>();

				solutionExplorer.Show();

				Assert.True(solutionExplorer.IsVisible);
			}

			[HostType("VS IDE")]
			[TestMethod]
			public void WhenGettingDefaultFactories_ThenGetsAllOfThem()
			{
                var exports = base.ServiceLocator.GetInstance<ExportProvider>();

                var withMetadata = exports.GetExports<ITreeNodeFactory<IVsSolutionHierarchyNode>, ITreeNodeFactoryMetadata>("SolutionExplorer").ToList();
                var withoutMetadata = exports.GetExports<ITreeNodeFactory<IVsSolutionHierarchyNode>>("SolutionExplorer").ToList();

                System.Diagnostics.Debugger.Launch();
				Assert.Equal(withoutMetadata.Count, withMetadata.Count);

				var fallbacks = withMetadata.Where(n => n.Metadata.IsFallback).ToList();

				Assert.True(fallbacks.Any(), "No fallback factories found. Factories with and without metadata found: {0}", withMetadata.Count);
			}

			[HostType("VS IDE")]
			[TestMethod]
			public void WhenGettingDefaultFactory_ThenCanRetrieveIt()
			{
                var defaultFactory = base.ServiceLocator.GetInstance<ITreeNodeFactory<IVsSolutionHierarchyNode>>(DefaultHierarchyFactory.RegisterKey);

				Assert.NotNull(defaultFactory);

				Assert.False(defaultFactory.Supports(new Mock<IVsSolutionHierarchyNode> { DefaultValue = DefaultValue.Mock }.Object));
			}
		}
	}
}
