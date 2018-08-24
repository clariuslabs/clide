using System.Linq;
using Microsoft.VisualStudio.Shell;
using Xunit;

namespace Clide.Adapters
{
    [Collection("OpenSolution11")]
    public class SolutionAdapterFacadeSpec
    {
        ISolutionFixture fixture;

        public SolutionAdapterFacadeSpec(OpenSolution11Fixture fixture)
        {
            this.fixture = fixture;
        }

        #region IVsHierarchyItem

        [VsFact]
        public void when_adapting_solution_as_vs_hierarchy_item_then_succeeds()
        {
            var from = fixture.Solution;

            var to = from.AsVsHierarchyItem();

            Assert.NotNull(to);
        }

        [VsFact]
        public void when_adapting_project__as_vs_hierarchy_item_then_succeeds()
        {
            var from = fixture.Solution.FindProject(x => x.Name == "CsLibrary");

            var to = from.AsVsHierarchyItem();

            Assert.NotNull(to);
        }

        [VsFact]
        public void when_adapting_item_as_vs_hierarchy_item_then_succeeds()
        {
            var from = fixture.Solution.FindProject(x => x.Name == "CsLibrary")
                .Nodes.OfType<IItemNode>().First(x => x.Name == "Class1.cs");

            var to = from.AsVsHierarchyItem();

            Assert.NotNull(to);
        }

        [VsFact]
        public void when_adapting_folder_as_vs_hierarchy_item_then_succeeds()
        {
            var from = fixture.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<IFolderNode>().First();

            var to = from.AsVsHierarchyItem();

            Assert.NotNull(to);
        }

        [VsFact]
        public void when_adapting_solution_folder_as_vs_hierarchy_item_then_succeeds()
        {
            var from = fixture.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<ISolutionFolderNode>().First();

            var to = from.AsVsHierarchyItem();

            Assert.NotNull(to);
        }

        [VsFact]
        public void when_adapting_references_as_vs_hierarchy_item_then_succeeds()
        {
            var from = fixture.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<IReferencesNode>().First();

            var to = from.AsVsHierarchyItem();

            Assert.NotNull(to);
        }

        [VsFact]
        public void when_adapting_reference_as_vs_hierarchy_item_then_succeeds()
        {
            var from = fixture.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<IReferenceNode>().First();

            var to = from.AsVsHierarchyItem();

            Assert.NotNull(to);
        }

        [VsFact]
        public void when_adapting_solution_item_as_vs_hierarchy_item_then_succeeds()
        {
            var from = fixture.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<ISolutionItemNode>().First();

            var to = from.AsVsHierarchyItem();

            Assert.NotNull(to);
        }

        #endregion

        #region IVsHierarchy

        [VsFact]
        public void when_adapting_solution_as_vs_hierarchy_then_succeeds()
        {
            var from = fixture.Solution;

            var to = from.AsVsHierarchy();

            Assert.NotNull(to);
        }

        [VsFact]
        public void when_adapting_project__as_vs_hierarchy_then_succeeds()
        {
            var from = fixture.Solution.FindProject(x => x.Name == "CsLibrary");

            var to = from.AsVsHierarchy();

            Assert.NotNull(to);
        }

        [VsFact]
        public void when_adapting_item_as_vs_hierarchy_then_succeeds()
        {
            var from = fixture.Solution.FindProject(x => x.Name == "CsLibrary")
                .Nodes.OfType<IItemNode>().First(x => x.Name == "Class1.cs");

            var to = from.AsVsHierarchy();

            Assert.NotNull(to);
        }

        [VsFact]
        public void when_adapting_folder_as_vs_hierarchy_then_succeeds()
        {
            var from = fixture.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<IFolderNode>().First();

            var to = from.AsVsHierarchy();

            Assert.NotNull(to);
        }

        [VsFact]
        public void when_adapting_solution_folder_as_vs_hierarchy_then_succeeds()
        {
            var from = fixture.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<ISolutionFolderNode>().First();

            var to = from.AsVsHierarchy();

            Assert.NotNull(to);
        }

        [VsFact]
        public void when_adapting_references_as_vs_hierarchy_then_succeeds()
        {
            var from = fixture.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<IReferencesNode>().First();

            var to = from.AsVsHierarchy();

            Assert.NotNull(to);
        }

        [VsFact]
        public void when_adapting_reference_as_vs_hierarchy_then_succeeds()
        {
            var from = fixture.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<IReferenceNode>().First();

            var to = from.AsVsHierarchy();

            Assert.NotNull(to);
        }

        [VsFact]
        public void when_adapting_solution_item_as_vs_hierarchy_then_succeeds()
        {
            var from = fixture.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<ISolutionItemNode>().First();

            var to = from.AsVsHierarchy();

            Assert.NotNull(to);
        }

        #endregion

        #region IVs*

        [VsFact]
        public void when_adapting_solution_as_vs_solution_then_succeeds()
        {
            var from = fixture.Solution;

            var to = from.AsVsSolution();

            Assert.NotNull(to);
        }

        [VsFact]
        public void when_adapting_project__as_vs_project_then_succeeds()
        {
            var from = fixture.Solution.FindProject(x => x.Name == "CsLibrary");

            var to = from.AsVsProject();

            Assert.NotNull(to);
        }


        #endregion

        #region VSLang

        [VsFact]
        public void when_adapting_project__as_vslang_then_succeeds()
        {
            var from = fixture.Solution.FindProject(x => x.Name == "CsLibrary");

            var to = from.AsVsLangProject();

            Assert.NotNull(to);
        }

        [VsFact]
        public void when_adapting_item_as_vslang_then_succeeds()
        {
            var from = fixture.Solution.FindProject(x => x.Name == "CsLibrary")
                .Nodes.OfType<IItemNode>().First(x => x.Name == "Class1.cs");

            var to = from.AsVsLangProjectItem();

            Assert.NotNull(to);
        }

        [VsFact]
        public void when_adapting_reference_as_vslang_then_succeeds()
        {
            var from = fixture.Solution.FindProject(x => x.Name == "CsLibrary")
                .Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<IReferenceNode>().First();

            var to = from.AsReference();

            Assert.NotNull(to);
        }

        [VsFact]
        public void when_adapting_references_as_vslang_then_succeeds()
        {
            var from = fixture.Solution.FindProject(x => x.Name == "CsLibrary")
                .Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<IReferenceNode>().First();

            var to = from.AsReference();

            Assert.NotNull(to);
        }

        #endregion
    }

}
