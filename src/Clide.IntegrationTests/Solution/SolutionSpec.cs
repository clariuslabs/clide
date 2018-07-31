using System.Linq;
using Microsoft.VisualStudio.Shell;
using Xunit;

namespace Clide.Solution
{
    [Trait("Feature", "Solution Traversal")]
    [Collection("OpenSolution11")]
    public class SolutionSpec
    {
        ISolutionFixture fixture;

        public SolutionSpec(OpenSolution11Fixture fixture)
        {
            this.fixture = fixture;
        }

        [VsixFact]
        public void when_finding_native_c_project_then_succeeds()
        {
            var folder = fixture.Solution.Nodes.FirstOrDefault(n => n.Text == "Native") as ISolutionFolderNode;
            Assert.NotNull(folder);

            var project = folder.Nodes.FirstOrDefault(n => n.Name == "CppLibrary") as IProjectNode;
            var nodes = folder.Nodes.ToList();
            Assert.NotNull(project);

            Assert.NotNull(project.OwningSolution);

            Assert.Same(fixture.Solution.As<IVsHierarchyItem>(), project.OwningSolution.As<IVsHierarchyItem>());
        }
    }
}