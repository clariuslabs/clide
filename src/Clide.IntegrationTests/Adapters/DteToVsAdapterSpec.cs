using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Xunit;

namespace Clide
{
	[Collection ("OpenSolution11")]
	public class DteToVsAdapterSpec
	{
		ISolutionFixture fixture;
		IAdapterService adapters;

		public DteToVsAdapterSpec (OpenSolution11Fixture fixture)
		{
			this.fixture = fixture;
			adapters = GlobalServiceLocator.Instance.GetExport<IAdapterService> ();
		}

		[VsixFact]
		public void when_adapting_solution_to_vssolution_then_succeeds ()
		{
			var from = GlobalServices.GetService<DTE>().Solution;

			var to = adapters.Adapt(from).As<IVsSolution>();

			Assert.NotNull (to);
		}

		[VsixFact]
		public void when_adapting_project_to_vsproject_then_succeeds ()
		{
			var from = ((IEnumerable)GlobalServices.GetService<DTE>().ActiveSolutionProjects).OfType<Project>().First();

			var to = adapters.Adapt(from).As<IVsProject>();

			Assert.NotNull (to);
		}

		[VsixFact]
		public void when_adapting_project_to_vshierarchy_then_succeeds ()
		{
			var from = ((IEnumerable)GlobalServices.GetService<DTE>().ActiveSolutionProjects).OfType<Project>().First();

			var to = adapters.Adapt(from).As<IVsHierarchy>();

			Assert.NotNull (to);
		}

		[VsixFact]
		public void when_adapting_project_to_hierarchyitem_then_succeeds ()
		{
			var from = ((IEnumerable)GlobalServices.GetService<DTE>().ActiveSolutionProjects).OfType<Project>().First();

			var to = adapters.Adapt(from).As<IVsHierarchyItem>();

			Assert.NotNull (to);
		}


		[VsixFact]
		public void when_adapting_projectitem_to_hierarchyitem_then_succeeds ()
		{
			var project = fixture.Solution.FindProject(x => x.Name == "CsLibrary").As<Project>();
			var from = project.ProjectItems.OfType<ProjectItem>()
				.First(x => x.Name == "Class1.cs");

			var to = adapters.Adapt(from).As<IVsHierarchyItem>();

			Assert.NotNull (to);
		}
	}
}
