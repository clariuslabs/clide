using System.Collections;
using System.Linq;
using EnvDTE;
using Xunit;

namespace Clide
{
    [Collection("OpenSolution11")]
    public class DteToVsAdapterFacadeSpec
    {
        ISolutionFixture fixture;

        public DteToVsAdapterFacadeSpec(OpenSolution11Fixture fixture)
        {
            this.fixture = fixture;
        }

        [VsFact]
        public void when_adapting_solution_to_vssolution_then_succeeds()
        {
            var from = GlobalServices.GetService<DTE>().Solution;

            var to = from.AsVsSolution();

            Assert.NotNull(to);
        }

        [InlineData("CsLibrary")]
        [InlineData("FsLibrary")]
        [InlineData("VbLibrary")]
        [InlineData("PclLibrary")]
        [VsTheory]
        public void when_adapting_project_to_vsproject_then_succeeds(string projectName)
        {
            var from = GlobalServices.GetService<DTE>().Solution.AllProjects().First(p => p.Name == projectName);

            var to = from.AsVsProject();

            Assert.NotNull(to);
        }

        [InlineData("CsLibrary")]
        [InlineData("FsLibrary")]
        [InlineData("VbLibrary")]
        [InlineData("PclLibrary")]
        [VsTheory]
        public void when_adapting_project_to_vshierarchy_then_succeeds(string projectName)
        {
            var from = GlobalServices.GetService<DTE>().Solution.AllProjects().First(p => p.Name == projectName);

            var to = from.AsVsHierarchy();

            Assert.NotNull(to);
        }

        [InlineData("CsLibrary")]
        [InlineData("FsLibrary")]
        [InlineData("VbLibrary")]
        [InlineData("PclLibrary")]
        [VsTheory]
        public void when_adapting_project_to_hierarchyitem_then_succeeds(string projectName)
        {
            var from = GlobalServices.GetService<DTE>().Solution.AllProjects().First(p => p.Name == projectName);

            var to = from.AsVsHierarchyItem();

            Assert.NotNull(to);
        }

        [VsFact]
        public void when_adapting_projectitem_to_hierarchyitem_then_succeeds()
        {
            var project = fixture.Solution.FindProject(x => x.Name == "CsLibrary").As<Project>();
            var from = project.ProjectItems.OfType<ProjectItem>()
                .First(x => x.Name == "Class1.cs");

            var to = from.AsVsHierarchyItem();

            Assert.NotNull(to);
        }
    }
}
