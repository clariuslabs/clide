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
    using Clide.Events;
    using Clide.Patterns.Adapter;
    using Microsoft.Practices.ServiceLocation;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Linq;

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
                    Mock.Of<IServiceLocator>(),
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
                    Mock.Of<IServiceLocator>(),
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
                    Mock.Of<IServiceLocator>(),
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
                    Mock.Of<IServiceLocator>(),
                    Mock.Of<IAdapterService>(),
                    Mock.Of<ISolutionEvents>());

                Assert.Null(solutionNode.Parent);
            }
		}

        [TestClass]
        public class GivenASolution : VsHostedSpec
        {
            private ISolutionNode solution;

            [TestInitialize]
            public override void TestInitialize()
            {
                base.TestInitialize();

                this.OpenSolution("SampleSolution\\SampleSolution.sln");
                this.solution = base.ServiceLocator.GetInstance<ISolutionExplorer>().Solution;
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
                var folder1 = solution.Nodes.FirstOrDefault(n => n.DisplayName == "SolutionFolder1") as ISolutionFolderNode;
                Assert.NotNull(folder1);
                Assert.NotNull(folder1.OwningSolution);

                var vbLib = folder1.Nodes.FirstOrDefault(n => n.DisplayName == "VBClassLibrary") as IProjectNode;
                Assert.NotNull(vbLib);
                Assert.NotNull(vbLib.OwningSolution);

                var vbRefs = vbLib.Nodes.FirstOrDefault(n => n.DisplayName == "References");
                Assert.NotNull(vbRefs);
                Assert.True(vbRefs.IsHidden);
                Assert.NotNull(vbLib.OwningSolution);

                var folder2 = folder1.Nodes.FirstOrDefault(n => n.DisplayName == "SolutionFolder2") as ISolutionFolderNode;
                Assert.NotNull(folder2);

                var csLib = folder2.Nodes.FirstOrDefault(n => n.DisplayName == "ClassLibrary") as IProjectNode;
                Assert.NotNull(csLib);

                var nodes = csLib.Nodes.Select(n => n.DisplayName).ToList();

                var folder = csLib.Nodes.FirstOrDefault(n => n.DisplayName == "Folder") as IFolderNode;
                Assert.NotNull(folder.Nodes.FirstOrDefault(n => n.DisplayName == "TextFile1.txt") as IItemNode);
                Assert.NotNull(csLib.Nodes.FirstOrDefault(n => n.DisplayName == "Class1.cs") as IItemNode);
                Assert.NotNull(folder.OwningSolution);

                Assert.Equal("ClassLibrary", ((IItemNode)folder.Nodes.FirstOrDefault(n => n.DisplayName == "TextFile1.txt")).OwningProject.DisplayName);
                Assert.Equal("ClassLibrary", ((IItemNode)csLib.Nodes.FirstOrDefault(n => n.DisplayName == "Class1.cs")).OwningProject.DisplayName);

                var references = csLib.Nodes.FirstOrDefault(n => n.DisplayName == "References") as IReferencesNode;
                Assert.NotNull(references, "No References node was exposed in the tree.");
                Assert.False(references.IsHidden);

                Assert.NotEqual(0, references.Nodes.Count());

                Assert.Equal("ClassLibrary", ((IReferencesNode)csLib.Nodes.FirstOrDefault(n => n.DisplayName == "References")).OwningProject.DisplayName);

                Assert.NotNull(references.Nodes.First() as IReferenceNode);

                var item = solution
                    .Nodes.First(node => node.DisplayName == "Solution Items")
                    .Nodes.First(node => node.DisplayName == "SolutionItem.txt");

                Assert.True(item is ISolutionItemNode);
                Assert.NotNull(((ISolutionItemNode)item).OwningSolution);
            }

            [HostType("VS IDE")]
            [TestMethod]
            public void WhenTraversingHiddenNode_ThenItIsHiddenAndInvisible()
            {
                var project = solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
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
                var project = solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                    .OfType<IProjectNode>()
                    .First(n => n.DisplayName == "VBClassLibrary");

                // In VB, References node is hidden, but exists in the hierarchy.
                var references = project.Nodes.OfType<IReferencesNode>().Single();
                var reference = references.Nodes.Single(node => node.DisplayName == "System.Xml.Linq");

                Assert.True(reference.IsHidden);
                Assert.False(reference.IsVisible);
            }

            [HostType("VS IDE")]
            [TestMethod]
            public void WhenItemFound_ThenOwningProjectIsValid()
            {
                var item = solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                    .OfType<IItemNode>()
                    .FirstOrDefault(node => node.DisplayName == "TextFile1.txt");

                Assert.NotNull(item);
                Assert.Equal("ClassLibrary", item.OwningProject.DisplayName);
            }

            [HostType("VS IDE")]
            [TestMethod]
            public void WhenFolderFound_ThenOwningProjectIsValid()
            {
                var target = solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                    .OfType<IFolderNode>()
                    .FirstOrDefault(node => node.DisplayName == "Folder");

                Assert.NotNull(target);
                Assert.Equal("ClassLibrary", target.OwningProject.DisplayName);
            }

            [HostType("VS IDE")]
            [TestMethod]
            public void WhenReferencesNode_ThenOwningProjectIsValid()
            {
                var target = solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                    .OfType<IReferencesNode>()
                    .FirstOrDefault(node => !node.IsHidden);

                Assert.NotNull(target);
                Assert.Equal("ClassLibrary", target.OwningProject.DisplayName);
            }

            [HostType("VS IDE")]
            [TestMethod]
            public void WhenReferenceFound_ThenOwningProjectIsValid()
            {
                var target = solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                    .OfType<IReferenceNode>()
                    .FirstOrDefault(node => !node.IsHidden && node.DisplayName == "System.Xml.Linq");

                Assert.NotNull(target);
                Assert.Equal("ClassLibrary", target.OwningProject.DisplayName);
            }

            [HostType("VS IDE")]
            [TestMethod]
            public void WhenSolutionItemFound_ThenOwningSolutionFolderIsValid()
            {
                solution.Nodes
                    .Traverse(TraverseKind.BreadthFirst, node => node.Nodes)
                    .ToList()
                    .ForEach(node => Console.WriteLine("{0} ({1})", node.DisplayName, node.GetType().Name));

                var target = solution.Nodes
                    .OfType<ISolutionFolderNode>()
                    .First()
                    .Nodes
                    .OfType<ISolutionItemNode>()
                    .FirstOrDefault();

                Assert.NotNull(target);
                Assert.Equal("Solution Items", target.OwningSolutionFolder.DisplayName);
            }

            [HostType("VS IDE")]
            [TestMethod]
            public void WhenProjectIsSelected_ThenCanRetrieveSelectedNode()
            {
                var project = solution.FindProjects().First();
                project.Select();

                var selection = solution.SelectedNodes.OfType<IProjectNode>().First();

                Assert.Equal(project.PhysicalPath, selection.PhysicalPath);
            }

            [HostType("VS IDE")]
            [TestMethod]
            public void WhenMultipleProjectsAreSelected_ThenCanRetrieveAllSelectedNodes()
            {
                var allProjects = solution.FindProjects().ToList();
                allProjects.ForEach(x => x.Select(true));

                var selection = solution.SelectedNodes.OfType<IProjectNode>().ToList();

                Assert.Equal(allProjects.Count, selection.Count);
            }

            [HostType("VS IDE")]
            [TestMethod]
            public void WhenSelectionIsSolution_ThenActiveProjectIsNull()
            {
                solution.Select();

                var active = solution.ActiveProject;

                Assert.Null(active);
            }

            [HostType("VS IDE")]
            [TestMethod]
            public void WhenNoSolutionIsOpened_ThenActiveProjectIsNull()
            {
                CloseSolution();

                var active = solution.ActiveProject;

                Assert.Null(active);
            }

            [HostType("VS IDE")]
            [TestMethod]
            public void WhenSingleProjectIsSelected_ThenActiveProjectIsNotNull()
            {
                var project = solution.FindProjects().First();
                project.Select();

                var active = solution.ActiveProject;

                Assert.NotNull(active);
            }

            [HostType("VS IDE")]
            [TestMethod]
            public void WhenSingleItemIsSelected_ThenOwningProjectIsActive()
            {
                solution.Traverse().OfType<IItemNode>().First().Select();

                var active = solution.ActiveProject;

                Assert.NotNull(active);
            }

            [HostType("VS IDE")]
            [TestMethod]
            public void WhenMultipleItemsInSingleHierarchyAreSelected_ThenOwningProjectIsActive()
            {
                var classes = solution.FindProjects().First()
                    .Traverse().OfType<IItemNode>()
                    .Where(i => i.DisplayName.EndsWith(".cs"))
                    .ToList();
                
                // First clear whatever other selection there it for another project
                classes[0].Select(false);
                classes.ForEach(i => i.Select(true));

                var active = solution.ActiveProject;

                Assert.NotNull(active);
            }

            [HostType("VS IDE")]
            [TestMethod]
            public void WhenMultipleItemsInDifferentHierarchiesAreSelected_ThenActiveProjectIsNull()
            {
                System.Diagnostics.Debugger.Launch();
                solution.FindProjects().First()
                    .Traverse().OfType<IItemNode>()
                    .Where(i => i.DisplayName.EndsWith(".cs"))
                    .First().Select();

                solution.FindProjects()
                    .SelectMany(p => p.Traverse())
                    .Where(i => i.DisplayName.EndsWith(".cs") || i.DisplayName.EndsWith(".vb"))
                    .OfType<IItemNode>()
                    .AsParallel().ForAll(i => i.Select(true));

                var active = solution.ActiveProject;

                Assert.Null(active);
            }
        }
	}
}