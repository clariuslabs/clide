#region BSD License
/* 
Copyright (c) 2012, Clarius Consulting
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

namespace Clide.Solution
{
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Diagnostics;

    [TestClass]
    public class GivenASolution : VsHostedSpec
    {
        internal static readonly IAssertion Assert = new Assertion();

        private ISolutionExplorer explorer;

        [TestInitialize]
        public override void TestInitialize()
        {
            base.TestInitialize();
            
            this.explorer = base.ServiceLocator.GetInstance<ISolutionExplorer>();
            this.OpenSolution("SampleSolution\\SampleSolution.sln");
            this.explorer.Solution.Select();
        }

        [TestCleanup]
        public override void TestCleanup()
        {
            base.TestCleanup();
            this.CloseSolution();
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenSelectingProjects_ThenSelectionReturnsOnlyProjectNodes()
        {
            var projects = explorer.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<IProjectNode>()
                .ToList();

            projects.First().Select(false);
            projects.Skip(1).ToList().ForEach(project => project.Select(true));

            Assert.True(projects.TrueForAll(project => project.IsSelected));

            var selection = explorer.SelectedNodes.ToList();

            Assert.Equal(projects.Count, selection.Count);
            Assert.True(selection.TrueForAll(node => node is IProjectNode));
            Assert.True(projects.TrueForAll(project => selection.OfType<IProjectNode>().Any(sel => sel.PhysicalPath == project.PhysicalPath)));
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenReplacingSelectionWithItem_ThenSelectionReturnsItemOnly()
        {
            explorer.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<IProjectNode>()
                .ToList()
                .ForEach(project => project.Select(true));

            Assert.True(explorer.SelectedNodes.Count() > 1);

            var item = explorer.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<IItemNode>()
                .First(node => node.DisplayName == "Class1.cs");

            item.Select();

            var selection = explorer.SelectedNodes.ToList();

            Assert.Equal(1, selection.Count);
            Assert.Equal(item.PhysicalPath, ((ItemNode)selection[0]).PhysicalPath);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenSelectingFolder_ThenSucceeds()
        {
            EnsureSelected<IFolderNode>("Folder");
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenSelectingSolutionFolder_ThenSucceeds()
        {
            EnsureSelected<ISolutionFolderNode>("SolutionFolder1");
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenSelectingSolution_ThenSucceeds()
        {
            EnsureSelected<ISolutionNode>("SampleSolution");
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenSelectingReferences_ThenSucceeds()
        {
            EnsureSelected<IReferencesNode>("References");
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenSelectingReference_ThenSucceeds()
        {
            EnsureSelected<IReferenceNode>("System.Xml.Linq");
        }

        private void EnsureSelected<TNode>(string displayName)
            where TNode : ISolutionExplorerNode
        {
            explorer.Solution.Expand(true);
            var target = new ITreeNode[] { explorer.Solution }.Concat(explorer.Solution.Nodes
                .Traverse(TraverseKind.DepthFirst, node => node.Nodes))
                .OfType<TNode>()
                .First(node => !node.IsHidden && node.DisplayName == displayName);

            target.Select();

            Assert.True(target.IsSelected);

            var selection = explorer.SelectedNodes.ToList();

            Assert.Equal(1, selection.Count);
            Assert.Equal(target.DisplayName, selection[0].DisplayName);
        }
    }
}
