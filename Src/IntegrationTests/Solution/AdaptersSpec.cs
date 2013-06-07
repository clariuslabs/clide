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
    using Clide;
    using Clide.Patterns.Adapter;
    using EnvDTE;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using VSLangProj;
    using MsBuild = Microsoft.Build.Evaluation;

    [TestClass]
    public class AdaptersSpec : VsHostedSpec
    {
        internal static readonly IAssertion Assert = new Assertion();

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenAdaptingUsingExtensionMethod_ThenSucceeds()
        {
            base.OpenSolution("SampleSolution\\SampleSolution.sln");

            var node = this.ServiceProvider.GetService<SVsSolution, IVsSolution>().Adapt().As<ISolutionNode>();

            Assert.NotNull(node);
        }


        [HostType("VS IDE")]
        [TestMethod]
        public void WhenSolutionIsOpened_ThenCanAdaptTypes()
        {
            var explorer = base.ServiceLocator.GetInstance<ISolutionExplorer>();

            base.OpenSolution("SampleSolution\\SampleSolution.sln");

            var folder1 = explorer.Solution.Nodes.FirstOrDefault(n => n.DisplayName == "SolutionFolder1") as ISolutionFolderNode;
            Assert.NotNull(folder1);

            var vbLib = folder1.Nodes.FirstOrDefault(n => n.DisplayName == "VBClassLibrary") as IProjectNode;
            Assert.NotNull(vbLib);

            Assert.NotNull(vbLib.As<EnvDTE.Project>());
            Assert.NotNull(vbLib.As<IVsHierarchy>());
            Assert.NotNull(vbLib.As<MsBuild.Project>());

            var folder2 = folder1.Nodes.FirstOrDefault(n => n.DisplayName == "SolutionFolder2") as ISolutionFolderNode;
            Assert.NotNull(folder2);

            Assert.NotNull(folder2.As<EnvDTE80.SolutionFolder>());
            Assert.NotNull(folder2.As<IVsHierarchy>());

            var csLib = folder2.Nodes.FirstOrDefault(n => n.DisplayName == "ClassLibrary") as IProjectNode;
            Assert.NotNull(csLib);

            Assert.NotNull(csLib.As<EnvDTE.Project>());
            Assert.NotNull(csLib.As<IVsHierarchy>());
            Assert.NotNull(csLib.As<MsBuild.Project>());

            var nodes = csLib.Nodes.Select(n => n.DisplayName).ToList();

            var folder = csLib.Nodes.FirstOrDefault(n => n.DisplayName == "Folder") as IFolderNode;
            Assert.NotNull(folder);

            Assert.NotNull(folder.As<EnvDTE.ProjectItem>());
            Assert.NotNull(folder.As<IVsHierarchy>());

            var item = folder.Nodes.FirstOrDefault(n => n.DisplayName == "TextFile1.txt") as IItemNode;
            Assert.NotNull(item);

            Assert.NotNull(item.As<EnvDTE.ProjectItem>());
            Assert.NotNull(item.As<IVsHierarchy>());
            Assert.NotNull(item.As<MsBuild.ProjectItem>());

            var references = csLib.Nodes.FirstOrDefault(n => n.DisplayName == "References") as IReferencesNode;
            Assert.NotNull(references, "No References node was exposed in the tree.");

            Assert.NotNull(references.As<References>());

            var reference = references.Nodes.First();
            Assert.NotNull(reference as IReferenceNode);

            Assert.NotNull(reference.As<Reference>());
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenAdaptingDteSolutionToISolutionNode_ThenSucceeds()
        {
            base.OpenSolution("SampleSolution\\SampleSolution.sln");

            var adapter = this.ServiceLocator.GetInstance<IAdapterService>();

            var node = adapter.Adapt(this.Dte.Solution).As<ISolutionNode>();

            Assert.NotNull(node);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenAdaptingDteProjectToIProjectNode_ThenSucceeds()
        {
            base.OpenSolution("SampleSolution\\SampleSolution.sln");

            var adapter = this.ServiceLocator.GetInstance<IAdapterService>();
            var explorer = base.ServiceLocator.GetInstance<ISolutionExplorer>();
            var library = explorer.Solution.Nodes.Traverse(TraverseKind.DepthFirst, x => x.Nodes)
                .OfType<IProjectNode>()
                .First(x => x.DisplayName == "ClassLibrary")
                .As<Project>();

            var node = adapter.Adapt(library).As<IProjectNode>();

            Assert.NotNull(node);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenAdaptingDteProjectItemToIItemNode_ThenSucceeds()
        {
            base.OpenSolution("SampleSolution\\SampleSolution.sln");

            var adapter = this.ServiceLocator.GetInstance<IAdapterService>();
            var explorer = base.ServiceLocator.GetInstance<ISolutionExplorer>();
            var file = explorer.Solution.Nodes.Traverse(TraverseKind.DepthFirst, x => x.Nodes)
                .OfType<IItemNode>()
                .First(x => x.DisplayName == "Class1.cs")
                .As<ProjectItem>();

            var node = adapter.Adapt(file).As<IItemNode>();

            Assert.NotNull(node);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenAdaptingIVsSolutionToISolutionNode_ThenSucceeds()
        {
            base.OpenSolution("SampleSolution\\SampleSolution.sln");

            var adapter = this.ServiceLocator.GetInstance<IAdapterService>();

            var node = adapter.Adapt(this.ServiceProvider.GetService<SVsSolution, IVsSolution>()).As<ISolutionNode>();

            Assert.NotNull(node);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenAdaptingIVsProjectToIProjectNode_ThenSucceeds()
        {
            base.OpenSolution("SampleSolution\\SampleSolution.sln");

            var adapter = this.ServiceLocator.GetInstance<IAdapterService>();
            var explorer = base.ServiceLocator.GetInstance<ISolutionExplorer>();
            var library = explorer.Solution.Nodes.Traverse(TraverseKind.DepthFirst, x => x.Nodes)
                .OfType<IProjectNode>()
                .First(x => x.DisplayName == "ClassLibrary")
                .As<IVsProject>();

            Assert.NotNull(library);

            var node = adapter.Adapt(library).As<IProjectNode>();

            Assert.NotNull(node);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenAdaptingVsHierarchyItemToISolutionNode_ThenSucceeds()
        {
            base.OpenSolution("SampleSolution\\SampleSolution.sln");

            var adapter = this.ServiceLocator.GetInstance<IAdapterService>();
            var explorer = base.ServiceLocator.GetInstance<ISolutionExplorer>();
            var source = explorer.Solution
                .As<VsHierarchyItem>();

            Assert.NotNull(source);

            var node = adapter.Adapt(source).As<ISolutionNode>();

            Assert.NotNull(node);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenAdaptingVsHierarchyItemToIProjectNode_ThenSucceeds()
        {
            base.OpenSolution("SampleSolution\\SampleSolution.sln");

            var adapter = this.ServiceLocator.GetInstance<IAdapterService>();
            var explorer = base.ServiceLocator.GetInstance<ISolutionExplorer>();
            var source = explorer.Solution.Nodes.Traverse(TraverseKind.DepthFirst, x => x.Nodes)
                .OfType<IProjectNode>()
                .First(x => x.DisplayName == "ClassLibrary")
                .As<VsHierarchyItem>();

            Assert.NotNull(source);

            var node = adapter.Adapt(source).As<IProjectNode>();

            Assert.NotNull(node);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenAdaptingVsHierarchyItemToIItemNode_ThenSucceeds()
        {
            base.OpenSolution("SampleSolution\\SampleSolution.sln");

            var adapter = this.ServiceLocator.GetInstance<IAdapterService>();
            var explorer = base.ServiceLocator.GetInstance<ISolutionExplorer>();
            var source = explorer.Solution.Nodes.Traverse(TraverseKind.DepthFirst, x => x.Nodes)
                .OfType<IItemNode>()
                .First(x => x.DisplayName == "Class1.cs")
                .As<VsHierarchyItem>();

            Assert.NotNull(source);

            var node = adapter.Adapt(source).As<IItemNode>();

            Assert.NotNull(node);
        }
    }
}
