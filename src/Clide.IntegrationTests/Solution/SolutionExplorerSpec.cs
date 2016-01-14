using Microsoft.VisualStudio.ComponentModelHost;
using Xunit;

namespace Clide.Solution.Explorer
{
	[Trait ("Feature", "Solution Traversal")]
	public class SolutionExplorerSpec
	{
		[VsixFact]
		public void when_getting_solution_explorer_then_succeeds ()
		{
			var solutionExplorer = GlobalServices.GetService<SComponentModel, IComponentModel>().GetService<ISolutionExplorer>();

			Assert.NotNull (solutionExplorer);
		}

		[VsixFact]
		public void when_closing_solution_explorer_then_is_visible_returns_false ()
		{
			var solutionExplorer = GlobalServices.GetService<SComponentModel, IComponentModel>().GetService<ISolutionExplorer>();

			solutionExplorer.Close ();

			Assert.False (solutionExplorer.IsVisible);
		}

		[VsixFact]
		public void when_closing_solution_explorer_then_is_visible_returns_true ()
		{
			var solutionExplorer = GlobalServices.GetService<SComponentModel, IComponentModel>().GetService<ISolutionExplorer>();

			solutionExplorer.Show ();

			Assert.True (solutionExplorer.IsVisible);
		}

		[VsixFact]
		public void when_getting_solution_node_then_returns_non_null ()
		{
			var solutionExplorer = GlobalServices.GetService<SComponentModel, IComponentModel>().GetService<ISolutionExplorer>();

			Assert.NotNull (solutionExplorer.Solution);
		}
	}
}