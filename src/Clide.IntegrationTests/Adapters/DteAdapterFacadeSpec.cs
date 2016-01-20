using System.Collections;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Xunit;

namespace Clide
{
	[Collection ("OpenSolution11")]
	public class DteToVsAdapterFacadeSpec
	{
		ISolutionFixture fixture;

		public DteToVsAdapterFacadeSpec (OpenSolution11Fixture fixture)
		{
			this.fixture = fixture;
		}

		[VsixFact]
		public void when_adapting_solution_to_vssolution_then_succeeds ()
		{
			var from = GlobalServices.GetService<DTE>().Solution;

			var to = from.AsVsSolution();

			Assert.NotNull (to);
		}

		[VsixFact]
		public void when_adapting_project_to_vsproject_then_succeeds ()
		{
			var from = ((IEnumerable)GlobalServices.GetService<DTE>().ActiveSolutionProjects).OfType<Project>().First();

			var to = from.AsVsProject();

			Assert.NotNull (to);
		}

		[VsixFact]
		public void when_adapting_project_to_vshierarchy_then_succeeds ()
		{
			var from = ((IEnumerable)GlobalServices.GetService<DTE>().ActiveSolutionProjects).OfType<Project>().First();

			var to = from.AsVsHierarchy();

			Assert.NotNull (to);
		}

		[VsixFact]
		public void when_adapting_project_to_hierarchyitem_then_succeeds ()
		{
			var from = ((IEnumerable)GlobalServices.GetService<DTE>().ActiveSolutionProjects).OfType<Project>().First();

			var to = from.AsVsHierarchyItem();

			Assert.NotNull (to);
		}

		[VsixFact]
		public void when_adapting_projectitem_to_hierarchyitem_then_succeeds ()
		{
			var project = fixture.Solution.FindProject(x => x.Name == "CsLibrary").As<Project>();
			var from = project.ProjectItems.OfType<ProjectItem>()
				.First(x => x.Name == "Class1.cs");

			var to = from.AsVsHierarchyItem();

			Assert.NotNull (to);
		}
	}
}
