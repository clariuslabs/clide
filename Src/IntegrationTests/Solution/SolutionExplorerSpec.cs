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
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;
    using Microsoft.VisualStudio.Shell;
    using Moq;
    using System.Diagnostics;

    public class SolutionExplorerSpec
	{
		internal static readonly IAssertion Assert = new Assertion();

		[TestClass]
		public class GivenNocontext : VsHostedSpec
		{
			[HostType("VS IDE")]
			[TestMethod]
			public void WhenGettingSolutionExplorer_ThenReturnsInstance()
			{
				var solutionExplorer = base.Container.GetExportedValue<ISolutionExplorer>();

				Assert.NotNull(solutionExplorer);
			}

			[HostType("VS IDE")]
			[TestMethod]
			public void WhenClosingSolutionExplorer_ThenIsOpenReturnsFalse()
			{
                var solutionExplorer = base.Container.GetExportedValue<ISolutionExplorer>();

				solutionExplorer.Close();

				Assert.False(solutionExplorer.IsVisible);
			}

			[HostType("VS IDE")]
			[TestMethod]
			public void WhenOpeningSolutionExplorer_ThenIsOpenReturnsTrue()
			{
                var solutionExplorer = base.Container.GetExportedValue<ISolutionExplorer>();

				solutionExplorer.Show();

				Assert.True(solutionExplorer.IsVisible);
			}

			[HostType("VS IDE")]
			[TestMethod]
			public void WhenGettingDefaultFactories_ThenGetsAllOfThem()
			{
                var withMetadata = base.Container.GetExports<ITreeNodeFactory<IVsSolutionHierarchyNode>, ITreeNodeFactoryMetadata>(CompositionTarget.SolutionExplorer).ToList();
                var withoutMetadata = base.Container.GetExports<ITreeNodeFactory<IVsSolutionHierarchyNode>>(CompositionTarget.SolutionExplorer).ToList();

				Assert.Equal(withoutMetadata.Count, withMetadata.Count);

				var fallbacks = withMetadata.Where(n => n.Metadata.IsFallback).ToList();

				Assert.True(fallbacks.Any());
			}

			[HostType("VS IDE")]
			[TestMethod]
			public void WhenGettingDefaultFactory_ThenCanRetrieveIt()
			{
                var defaultFactory = base.Container.GetExportedValue<ITreeNodeFactory<IVsSolutionHierarchyNode>>(DefaultHierarchyFactory.ContractName);

				Assert.NotNull(defaultFactory);

				Assert.False(defaultFactory.Supports(new Mock<IVsSolutionHierarchyNode> { DefaultValue = DefaultValue.Mock }.Object));
			}
		}

        [TestClass]
        public class GivenASolution : VsHostedSpec
        {
            private ISolutionExplorer explorer;

            [TestInitialize]
            public override void TestInitialize()
            {
                base.TestInitialize();

                this.explorer = base.Container.GetExportedValue<ISolutionExplorer>();
                this.OpenSolution("SampleSolution\\SampleSolution.sln");
            }

            [TestCleanup]
            public override void TestCleanup()
            {
                base.TestCleanup();
                this.CloseSolution();
            }

            [HostType("VS IDE")]
            [TestMethod]
            public void WhenSolutionIsOpened_ThenReturnsProperHierarchy()
            {
                var folder1 = explorer.Solution.Nodes.FirstOrDefault(n => n.DisplayName == "SolutionFolder1") as ISolutionFolderNode;
                Assert.NotNull(folder1);

                var vbLib = folder1.Nodes.FirstOrDefault(n => n.DisplayName == "VBClassLibrary") as IProjectNode;
                Assert.NotNull(vbLib);
                var vbRefs = vbLib.Nodes.FirstOrDefault(n => n.DisplayName == "References");
                //Assert.NotNull(vbRefs);

                var folder2 = folder1.Nodes.FirstOrDefault(n => n.DisplayName == "SolutionFolder2") as ISolutionFolderNode;
                Assert.NotNull(folder2);

                var csLib = folder2.Nodes.FirstOrDefault(n => n.DisplayName == "ClassLibrary") as IProjectNode;
                Assert.NotNull(csLib);

                var nodes = csLib.Nodes.Select(n => n.DisplayName).ToList();

                var folder = csLib.Nodes.FirstOrDefault(n => n.DisplayName == "Folder") as IFolderNode;

                Assert.NotNull(folder);

                Assert.NotNull(folder.Nodes.FirstOrDefault(n => n.DisplayName == "TextFile1.txt") as IItemNode);
                Assert.NotNull(csLib.Nodes.FirstOrDefault(n => n.DisplayName == "Class1.cs") as IItemNode);

                var references = csLib.Nodes.FirstOrDefault(n => n.DisplayName == "References");
                Assert.NotNull(references, "No References node was exposed in the tree.");

                //Assert.True(references.Is(SolutionNodeKind.ReferencesFolder));
                Assert.NotEqual(0, references.Nodes.Count());

                references.Nodes.ToList().ForEach(t => System.Console.WriteLine(t.DisplayName));

                Assert.NotNull(references.Nodes.First() as IReferenceNode);
            }

            [HostType("VS IDE")]
            [TestMethod]
            public void WhenTraversingHiddenNode_ThenItIsHiddenAndInvisible()
            {
                var project = this.explorer.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                    .OfType<IProjectNode>()
                    .First(n => n.DisplayName == "VBClassLibrary");

                // In VB, References node is hidden, but exists in the hierarchy.
                var references = project.Nodes.Single(node => node.DisplayName == "References");

                Assert.True(references.IsHidden);
                Assert.False(references.IsVisible);
            }

            [HostType("VS IDE")]
            [TestMethod]
            public void WhenTraversingHiddenDescendentNode_ThenItIsHiddenAndInvisible()
            {
                var project = this.explorer.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                    .OfType<IProjectNode>()
                    .First(n => n.DisplayName == "VBClassLibrary");

                // In VB, References node is hidden, but exists in the hierarchy.
                var references = project.Nodes.OfType<IReferencesNode>().Single();
                var reference = references.Nodes.Single(node => node.DisplayName == "System.Xml.Linq");

                Assert.True(reference.IsHidden);
                Assert.False(reference.IsVisible);
            }
        }
	}
}
