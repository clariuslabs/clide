namespace Clide.HowTo.Adapters
{
	using EnvDTE;
	using Clide.Solution;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Microsoft.VSSDK.Tools.VsIdeTesting;
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	[TestClass]
	[DisplayName(
@"## How To: convert back and forth between DTE, IVsSolution, IVsProject and Clide APIs
")]
	public partial class AdaptProject : VsHostedSpec
	{
		const string baseUrl = "Adapters/AdaptProject.cs#L";

		[DisplayName("*  [Convert DTE Project to IProjectNode](" + baseUrl + "26)")]
		[HostType("VS IDE")]
		[TestMethod]
		public void how_to_convert_DTE_project_to_IProjectNode()
		{
			// Get DTE project, in this case by finding it by name using DTE APIs.
			EnvDTE.Project project = ((IEnumerable)Dte.ActiveSolutionProjects)
				.OfType<EnvDTE.Project>()
				.First(p => p.Name == "ClassLibrary");

			IProjectNode node = project.Adapt().AsProjectNode();

			Assert.IsNotNull(node);

			// Now we can use Clide's project node API, such as accessing 
			// the MSBuild properties using dynamic syntax.
			string assemblyName = node.Properties.AssemblyName;

			Assert.AreEqual("ClassLibrary", assemblyName);


			// Or a property for a specific configuration
			string debugType = node.PropertiesFor("Debug|AnyCPU").DebugType;

			Assert.AreEqual("full", debugType);
		}



		[TestInitialize]
		public override void TestInitialize()
		{
			base.TestInitialize();

			base.OpenSolution("SampleSolution\\SampleSolution.sln");

			Solution = DevEnv.Get(ServiceProvider).SolutionExplorer().Solution;
			ClassLibrary = Solution.FindProjects(p => p.DisplayName == "ClassLibrary").First();
		}

		public ISolutionNode Solution { get; private set; }
		public IProjectNode ClassLibrary { get; private set; }


	}
}
