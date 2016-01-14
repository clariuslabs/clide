using System;
using System.IO;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Clide.Interop;
using Xunit;

namespace Clide.Solution
{
	[Trait ("Feature", "Solution Traversal")]
	public class SolutionNodeSpec : IDisposable
	{
		ISolutionNode solution;

		public SolutionNodeSpec ()
		{
			var components = GlobalServices.GetService<SComponentModel, IComponentModel>();
			var factory = components.GetService<ISolutionExplorerNodeFactory> ();
			var adapter = components.GetService<IAdapterService> ();
			var manager = components.GetService<IVsHierarchyItemManager>();
			var selection = components.GetService<IVsSolutionSelection>();
			var solutionExplorer = components.DefaultExportProvider.GetExport<IVsUIHierarchyWindow>(ContractNames.Interop.SolutionExplorerWindow);
			var item = manager.GetHierarchyItem(GlobalServices.GetService<SVsSolution, IVsHierarchy>(), (uint)VSConstants.VSITEMID.Root);

			solution = new SolutionNode (item, factory, adapter, selection, solutionExplorer);
		}

		public void Dispose ()
		{
			if (solution.IsOpen)
				solution.Close (false);
		}

		[VsixFact]
		public void when_closing_and_no_solution_open_then_succeeds ()
		{
			if (solution.IsOpen)
				solution.Close ();

			solution.Close ();
		}

		[VsixFact]
		public void when_creating_solution_then_is_open_returns_true ()
		{
			solution.Create (new FileInfo (Path.Combine (Environment.GetEnvironmentVariable ("VisualStudioVersion"), "foo.sln")).FullName);

			Assert.True (solution.IsOpen);
		}

		[VsixFact]
		public void when_solution_then_can_save_as_different_name ()
		{
			var dir = new DirectoryInfo(Path.Combine(".", Environment.GetEnvironmentVariable ("VisualStudioVersion"))).FullName;
			solution.Create (Path.Combine (dir, "foo.sln"));

			Assert.True (File.Exists (Path.Combine (dir, "foo.sln")));

			solution.SaveAs (Path.Combine (dir, "bar.sln"));

			Assert.True (File.Exists (Path.Combine (dir, "bar.sln")));
		}

		[VsixFact]
		public void when_creating_solution_with_invalid_name_then_throws ()
		{
			Assert.Throws<ArgumentException> (() => solution.Create ("foo"));
		}

		[VsixFact]
		public void when_solution_is_opened_then_is_open_returns_true ()
		{
			solution.Open (Constants.BlankSolution);

			Assert.True (solution.IsOpen);
		}

		[VsixFact]
		public void when_getting_parent_then_returns_null ()
		{
			Assert.Null (solution.Parent);
		}
	}
}