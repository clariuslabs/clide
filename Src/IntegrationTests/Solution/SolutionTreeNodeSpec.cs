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
    using Clide.Patterns.Adapter;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Linq;

    internal class SolutionTreeNodeSpec
	{
		internal static readonly IAssertion Assert = new Assertion();

		[TestClass]
		public class GivenASolution : VsHostedSpec
		{
			private SolutionTreeNode node;

			[TestInitialize]
			public override void TestInitialize()
			{
				base.TestInitialize();

				base.OpenSolution(GetFullPath("SampleSolution\\SampleSolution.sln"));

				var adapter = new Mock<IAdapterService>();
                //adapter.Setup(x => x.As<ISolutionNode>(It.IsAny<object>()))
                //    .Returns<object>(o => (ISolutionNode)o);
                adapter.Setup(x => x.Adapt(It.IsAny<object>()))
                    .Returns<object>(o => Mock.Of<IAdaptable<object>>(a => a.As<ISolutionNode>() == (ISolutionNode)o));

				var solution = ServiceProvider.GetService<IVsSolution>();
				var hierarchy = solution as IVsHierarchy;
				var factory = new Mock<ITreeNodeFactory<IVsSolutionHierarchyNode>>();
				factory.Setup(x => x.Supports(It.IsAny<IVsSolutionHierarchyNode>())).Returns(true);
				factory.Setup(x => x.CreateNode(It.IsAny<Lazy<ITreeNode>>(), It.IsAny<IVsSolutionHierarchyNode>()))
					.Returns<Lazy<ITreeNode>, IVsSolutionHierarchyNode>((parent, node) => new CustomSolutionNode(
						node,
						parent,
						factory.Object,
						adapter.Object));

                this.node = new CustomSolutionNode(
                    new VsSolutionHierarchyNode(hierarchy, VSConstants.VSITEMID_ROOT), null,
					factory.Object,
					Mock.Of<IAdapterService>());
			}

			[HostType("VS IDE")]
			[TestMethod]
			public void WhenNodeIsSolution_ThenIsVisibleReturnsTrue()
			{
				Assert.True(node.IsVisible);
			}

			[HostType("VS IDE")]
			[TestMethod]
			public void WhenParentNodeIsCollapsed_ThenChildNodeIsVisibleFalse()
			{
				var file = node.Nodes
					.Select(n => { Console.WriteLine(n.DisplayName); return n; })
					.First(n => n.DisplayName == "SolutionFolder1")
					.Nodes
					.First(n => n.DisplayName == "SolutionFolder2")
					.Nodes
					.First()
					.Nodes
					.First(n => n.DisplayName == "Folder")
					.Nodes
					.First(n => n.DisplayName == "TextFile1.txt");

				file.Parent.Expand();
				file.Parent.Collapse();
				Assert.False(file.IsVisible);
			}

			[HostType("VS IDE")]
			[TestMethod]
			public void WhenParentNodeIsExpanded_ThenChildNodeIsVisibleTrue()
			{
				var file = node.Nodes
					.First(n => n.DisplayName == "SolutionFolder1")
					.Nodes
					.First(n => n.DisplayName == "SolutionFolder2")
					.Nodes
					.First()
					.Nodes
					.First(n => n.DisplayName == "Folder")
					.Nodes
					.First(n => n.DisplayName == "TextFile1.txt");

				file.Parent.Expand();
				Assert.True(file.IsVisible);
			}

			[HostType("VS IDE")]
			[TestMethod]
			public void WhenExpandingNode_ThenNodeIsExpanded()
			{
				var file = node.Nodes
					.First(n => n.DisplayName == "SolutionFolder1")
					.Nodes
					.First(n => n.DisplayName == "SolutionFolder2")
					.Nodes
					.First()
					.Nodes
					.First(n => n.DisplayName == "Folder")
					.Nodes
					.First(n => n.DisplayName == "TextFile1.txt");

                file.Parent.Collapse();
                Assert.False(file.Parent.IsExpanded);
				file.Parent.Expand();
				Assert.True(file.Parent.IsExpanded);
			}

			[HostType("VS IDE")]
			[TestMethod]
			public void WhenCollapsingNode_ThenNodeIsNotExpanded()
			{
				var file = node.Nodes
					.First(n => n.DisplayName == "SolutionFolder1")
					.Nodes
					.First(n => n.DisplayName == "SolutionFolder2")
					.Nodes
					.First()
					.Nodes
					.First(n => n.DisplayName == "Folder")
					.Nodes
					.First(n => n.DisplayName == "TextFile1.txt");

				file.Parent.Collapse();
				Assert.False(file.Parent.IsExpanded);
			}

			[HostType("VS IDE")]
			[TestMethod]
			public void WhenSelectingNode_ThenNodeIsSelected()
			{
				var file = node.Nodes
					.First(n => n.DisplayName == "SolutionFolder1")
					.Nodes
					.First(n => n.DisplayName == "SolutionFolder2")
					.Nodes
					.First()
					.Nodes
					.First(n => n.DisplayName == "Folder")
					.Nodes
					.First(n => n.DisplayName == "TextFile1.txt");

				file.Parent.Expand();
				Assert.False(file.IsSelected);
				file.Select(false);
				Assert.True(file.IsSelected);
			}
		}

        public class CustomSolutionNode : SolutionTreeNode
        {
            public CustomSolutionNode(IVsSolutionHierarchyNode node, 
                Lazy<ITreeNode> parent, 
                ITreeNodeFactory<IVsSolutionHierarchyNode> nodeFactory, 
                IAdapterService adapterService)
                : base(SolutionNodeKind.Custom, node, parent, nodeFactory, adapterService)
            {
            }

            public override bool Accept(ISolutionVisitor visitor)
            {
                return true;
            }
        }
	}
}
