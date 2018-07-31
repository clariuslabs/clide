using Xunit;
using System.Linq;
using Xunit.Abstractions;

namespace Clide.Solution
{
    [Trait("LongRunning", "true")]
    [Vsix(MinimumVisualStudioVersion = VisualStudioVersion.VS2013)]
    [Collection("OpenCopySolution")]
    public class SharedProjectSpec
    {
        ISolutionFixture fixture;
        ITestOutputHelper output;

        public SharedProjectSpec(OpenCopySolutionFixture fixture, ITestOutputHelper output)
        {
            this.fixture = fixture;
            this.output = output;
        }

        [VsixFact]
        public void when_comparing_shared_projects_then_can_check_for_equality()
        {
            var first = fixture.Solution.FindProject(x => x.Name == "CsShared");
            var second = fixture.Solution.FindProject(x => x.Name == "CsShared");

            Assert.Equal(first, second);
        }

        [VsixFact]
        public void when_comparing_items_in_shared_projects_then_can_check_for_equality()
        {
            var first = fixture.Solution.FindProject(x => x.Name == "CsShared")
                .Nodes.OfType<IItemNode>().First(x => x.Text == "SharedClass1.cs");
            var second = fixture.Solution.FindProject(x => x.Name == "CsShared")
                .Nodes.OfType<IItemNode>().First(x => x.Text == "SharedClass1.cs");

            Assert.Equal(first, second);
        }

        [VsixFact]
        public void when_getting_logical_path_of_item_then_succeeds_for_shared_project()
        {
            var node = fixture.Solution.FindProject(x => x.Name == "CsShared")
                .Nodes.Traverse(TraverseKind.DepthFirst, x => x.Nodes)
                .OfType<IItemNode>().First(x => x.Text == "TextFile1.txt");

            Assert.NotNull(node);

            Assert.Equal(node.OwningProject, node.OwningProject);

            var project = node.OwningProject;
            var ancestors = node.Ancestors().ToList();

            Assert.True(ancestors.Contains(project), "Ancestors doesn't contain owning project.");

            Assert.Equal("CsSharedFolder\\TextFile1.txt", node.LogicalPath);
        }
    }
}
