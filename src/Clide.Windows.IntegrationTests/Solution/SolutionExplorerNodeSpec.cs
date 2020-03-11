using Xunit;
using System;
using System.Linq;

namespace Clide.Solution
{
    [Trait("LongRunning", "true")]
    [Trait("Feature", "Solution Traversal")]
    [Collection("OpenSolution11")]
    public class SolutionExplorerNodeSpec
    {
        ISolutionFixture fixture;

        public SolutionExplorerNodeSpec(OpenSolution11Fixture fixture)
        {
            this.fixture = fixture;
        }

        [VsixFact]
        public void when_node_is_solution_then_is_visible_returns_true()
        {
            Assert.True(fixture.Solution.IsVisible);
        }

        [InlineData("CsLibrary", "Class1.cs")]
        [InlineData("NsLibrary", "Class1.cs")]
        [VsixTheory]
        public void when_parent_node_is_collapsed_then_child_node_is_visible_false(string projectName, string fileName)
        {
            var file = fixture.Solution.FindProject(x => x.Name == projectName)
                .Nodes.OfType<IItemNode>().First(x => x.Name == fileName);

            file.Parent.Expand();
            file.Parent.Collapse();
            Assert.False(file.IsVisible);
        }

        [InlineData("CsLibrary", "Class1.cs")]
        [InlineData("NsLibrary", "Class1.cs")]
        [VsixTheory]
        public void when_parent_node_is_expanded_then_child_node_is_visible_true(string projectName, string fileName)
        {
            var file = fixture.Solution.FindProject(x => x.Name == projectName)
                .Nodes.OfType<IItemNode>().First(x => x.Name == fileName);

            file.Parent.Collapse();
            file.Parent.Expand();
            Assert.True(file.IsVisible);
        }

        [InlineData("CsLibrary", "Class1.cs")]
        [InlineData("NsLibrary", "Class1.cs")]
        [VsixTheory]
        public void when_expanding_node_then_node_is_expanded(string projectName, string fileName)
        {
            var file = fixture.Solution.FindProject(x => x.Name == projectName)
                .Nodes.OfType<IItemNode>().First(x => x.Name == fileName);

            file.Parent.Collapse();
            Assert.False(file.Parent.IsExpanded);
            file.Parent.Expand();
            Assert.True(file.Parent.IsExpanded);
        }

        [InlineData("CsLibrary", "Class1.cs")]
        [InlineData("NsLibrary", "Class1.cs")]
        [VsixTheory]
        public void when_collapsing_node_then_node_is_not_expanded(string projectName, string fileName)
        {
            var file = fixture.Solution.FindProject(x => x.Name == projectName)
                .Nodes.OfType<IItemNode>().First(x => x.Name == fileName);

            file.Parent.Collapse();
            Assert.False(file.Parent.IsExpanded);
        }

        [InlineData("CsLibrary", "Class1.cs")]
        [InlineData("NsLibrary", "Class1.cs")]
        [VsixTheory]
        public void when_selecting_node_then_node_is_selected(string projectName, string fileName)
        {
            var file = fixture.Solution.FindProject(x => x.Name == projectName)
                .Nodes.OfType<IItemNode>().First(x => x.Name == fileName);

            file.Parent.Expand();
            Assert.False(file.IsSelected);
            file.Select(false);
            Assert.True(file.IsSelected);
        }
    }
}
