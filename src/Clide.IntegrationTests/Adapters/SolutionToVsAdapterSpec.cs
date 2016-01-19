using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Xunit;

namespace Clide.Adapters
{
	[Collection ("OpenSolution11")]
	public class SolutionToVsAdapterSpec
	{
		ISolutionFixture fixture;
		IAdapterService adapters;

		public SolutionToVsAdapterSpec (OpenSolution11Fixture fixture)
		{
			this.fixture = fixture;
			adapters = GlobalServiceLocator.Instance.GetExport<IAdapterService> ();
		}

		[VsixFact]
		public void when_adapting_solution_then_succeeds ()
		{
			var from = fixture.Solution;

			var to = adapters.Adapt(from).As<IVsHierarchyItem>();

			Assert.NotNull (to);
		}

		[VsixFact]
		public void when_adapting_project_then_succeeds ()
		{
			var from = fixture.Solution.FindProject(x => x.Name == "CsLibrary");

			var to = adapters.Adapt(from).As<IVsHierarchyItem>();

			Assert.NotNull (to);
		}

		[VsixFact]
		public void when_adapting_item_then_succeeds ()
		{
			var from = fixture.Solution.FindProject(x => x.Name == "CsLibrary")
				.Nodes.OfType<IItemNode>().First(x => x.Name == "Class1.cs");

			var to = adapters.Adapt(from).As<IVsHierarchyItem>();

			Assert.NotNull (to);
		}

		[VsixFact]
		public void when_adapting_folder_then_succeeds ()
		{
			var from = fixture.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
				.OfType<IFolderNode>().First();

			var to = adapters.Adapt(from).As<IVsHierarchyItem>();

			Assert.NotNull (to);
		}

		[VsixFact]
		public void when_adapting_solution_folder_then_succeeds ()
		{
			var from = fixture.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
				.OfType<ISolutionFolderNode>().First();

			var to = adapters.Adapt(from).As<IVsHierarchyItem>();

			Assert.NotNull (to);
		}

		[VsixFact]
		public void when_adapting_references_then_succeeds ()
		{
			var from = fixture.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
				.OfType<IReferencesNode>().First();

			var to = adapters.Adapt(from).As<IVsHierarchyItem>();

			Assert.NotNull (to);
		}

		[VsixFact]
		public void when_adapting_reference_then_succeeds ()
		{
			var from = fixture.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
				.OfType<IReferenceNode>().First();

			var to = adapters.Adapt(from).As<IVsHierarchyItem>();

			Assert.NotNull (to);
		}

		[VsixFact]
		public void when_adapting_solution_item_then_succeeds ()
		{
			var from = fixture.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
				.OfType<ISolutionItemNode>().First();

			var to = adapters.Adapt(from).As<IVsHierarchyItem>();

			Assert.NotNull (to);
		}
	}
}
