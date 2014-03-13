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
	using Microsoft.VisualStudio.Shell.Interop;
	using Microsoft.VisualStudio;

	[TestClass]
	[DisplayName(
@"## Convert between APIs
")]
	public partial class AdaptProject : VsHostedSpec
	{
		const string baseUrl = "Adapters/AdaptProject.cs#L";

		[DisplayName("*  [Convert IVsSolution to Clide's ISolutionNode](" + baseUrl + "33)")]
		[HostType("VS IDE")]
		[TestMethod]
		public void how_to_convert_IVsSolution_ISolutionNode()
		{
			// Say you have an IVsSolution:
			var vsSolution = (IVsSolution)ServiceProvider.GetService(typeof(SVsSolution));

			ISolutionNode solutionNode = vsSolution.Adapt().AsSolutionNode();

			Assert.IsNotNull(solutionNode);
			
			// Use the solution node to get the active project, for example:
			IProjectNode activeProject = solutionNode.ActiveProject;
		}

		[DisplayName("*  [Convert IVsSolution to DTE Solution](" + baseUrl + "49)")]
		[HostType("VS IDE")]
		[TestMethod]
		public void how_to_convert_IVsSolution_DTE_Solution()
		{
			// Say you have an IVsSolution:
			var vsSolution = (IVsSolution)ServiceProvider.GetService(typeof(SVsSolution));

			EnvDTE.Solution dteSolution = vsSolution.Adapt().AsDteSolution();

			Assert.IsNotNull(dteSolution);
		}

		[DisplayName("*  [Convert DTE Solution to IVsSolution](" + baseUrl + "62)")]
		[HostType("VS IDE")]
		[TestMethod]
		public void how_to_convert_DTE_Solution_to_IVsSolution()
		{
			// Say you have a DTE Solution:
			EnvDTE.Solution dteSolution = Dte.Solution;

			var vsSolution = dteSolution.Adapt().AsVsSolution();

			Assert.IsNotNull(dteSolution);
		}

		[DisplayName("*  [Convert DTE Solution to Clide's ISolutionNode](" + baseUrl + "75)")]
		[HostType("VS IDE")]
		[TestMethod]
		public void how_to_convert_DTE_Solution_to_ISolutionNode()
		{
			// Say you have a DTE Solution:
			EnvDTE.Solution dteSolution = Dte.Solution;

			ISolutionNode solutionNode = dteSolution.Adapt().AsSolutionNode();

			Assert.IsNotNull(solutionNode);

			// Use the solution node to get the active project, for example:
			IProjectNode activeProject = solutionNode.ActiveProject;
		}

		[DisplayName("*  [Convert DTE Project to MSBuild Project](" + baseUrl + "91)")]
		[HostType("VS IDE")]
		[TestMethod]
		public void how_to_convert_DTE_project_to_MsBuildProject()
		{
			// Say you got a DTE project somehow.
			EnvDTE.Project dteProject = this.DteLibrary;

			Microsoft.Build.Evaluation.Project msbuildProject = dteProject.Adapt().AsMsBuildProject();

			Assert.IsNotNull(msbuildProject);
			// Access the MSBuild imports, for example
			Assert.IsTrue(msbuildProject.Imports.Count > 0);
		}

		[DisplayName("*  [Convert DTE Project to IVsProject](" + baseUrl + "106)")]
		[HostType("VS IDE")]
		[TestMethod]
		public void how_to_convert_DTE_project_to_IVsProject()
		{
			// Say you got a DTE project somehow.
			EnvDTE.Project dteProject = this.DteLibrary;

			IVsProject vsProject = dteProject.Adapt().AsVsProject();

			Assert.IsNotNull(vsProject);
			
			// Use the VS project to open an item in a specific view, the designer, for example
			uint itemId = 0; // Get the item ID to open somehow, see other how-tos.
			IVsWindowFrame frame;
			Guid viewId = VSConstants.LOGVIEWID.Designer_guid;
			vsProject.OpenItem(itemId, ref viewId, IntPtr.Zero, out frame);
		}

		[DisplayName("*  [Convert DTE Project to VSLangProj.VSProject (C# or VB project)](" + baseUrl + "125)")]
		[HostType("VS IDE")]
		[TestMethod]
		public void how_to_convert_DTE_project_to_VSProject()
		{
			// Say you got a DTE project somehow.
			EnvDTE.Project dteProject = this.DteLibrary;

			VSLangProj.VSProject vsProject = dteProject.Adapt().AsVsLangProject();

			Assert.IsNotNull(vsProject);

			// Use the project, for example, to create the Web References folder
			ProjectItem folder = vsProject.CreateWebReferencesFolder();
			Assert.IsNotNull(folder);
		}

		[DisplayName("*  [Convert DTE Project to Clide's IProjectNode](" + baseUrl + "142)")]
		[HostType("VS IDE")]
		[TestMethod]
		public void how_to_convert_DTE_project_to_IProjectNode()
		{
			// Say you got a DTE project somehow.
			EnvDTE.Project dteProject = this.DteLibrary;

			IProjectNode projectNode = dteProject.Adapt().AsProjectNode();

			Assert.IsNotNull(projectNode);

			// Now we can use Clide's project node API, such as accessing 
			// the MSBuild properties using dynamic syntax.
			string assemblyName = projectNode.Properties.AssemblyName;

			Assert.AreEqual("ClassLibrary", assemblyName);

			// Or a property for a specific configuration
			string debugType = projectNode.PropertiesFor("Debug|AnyCPU").DebugType;

			Assert.AreEqual("full", debugType);
		}

		[DisplayName("*  [Convert IVsProject to Clide's IProjectNode](" + baseUrl + "166)")]
		[HostType("VS IDE")]
		[TestMethod]
		public void how_to_convert_IVsProject_project_to_IProjectNode()
		{
			// Say you got an IVsProject somehow.
			IVsProject vsProject = this.IVsLibrary;

			IProjectNode projectNode = vsProject.Adapt().AsProjectNode();

			Assert.IsNotNull(projectNode);

			// Now we can use Clide's project node API, such as accessing 
			// retrieving the active platform

			Assert.AreEqual("AnyCPU", projectNode.Configuration.ActivePlatform);
		}

		[DisplayName("*  [Convert IVsProject to DTE Project](" + baseUrl + "184)")]
		[HostType("VS IDE")]
		[TestMethod]
		public void how_to_convert_IVsProject_to_DTE_project()
		{
			// Say you got an IVsProject somehow.
			IVsProject vsProject = this.IVsLibrary;

			EnvDTE.Project dteProject = vsProject.Adapt().AsDteProject();

			Assert.IsNotNull(dteProject);

			// Use the DTE project API, such as to save the project:
			dteProject.Save();
		}

		[DisplayName("*  [Convert IVsProject to MSBuild Project](" + baseUrl + "200)")]
		[HostType("VS IDE")]
		[TestMethod]
		public void how_to_convert_IVsProject_to_MsBuildProject()
		{
			// Say you got an IVsProject somehow.
			IVsProject vsProject = this.IVsLibrary;

			Microsoft.Build.Evaluation.Project msbuildProject = vsProject.Adapt().AsMsBuildProject();

			Assert.IsNotNull(msbuildProject);

			// Access the MSBuild imports, for example
			Assert.IsTrue(msbuildProject.Imports.Count > 0);
		}

		[DisplayName("*  [Convert IVsProject to VSLangProj.VSProject (C# or VB project)](" + baseUrl + "216)")]
		[HostType("VS IDE")]
		[TestMethod]
		public void how_to_convert_IVsProject_to_VSProject()
		{
			// Say you got an IVsProject somehow.
			IVsProject vsProject = this.IVsLibrary;

			VSLangProj.VSProject langProject = vsProject.Adapt().AsVsLangProject();

			Assert.IsNotNull(langProject);

			// Use the project, for example, to create the Web References folder
			ProjectItem folder = langProject.CreateWebReferencesFolder();
			Assert.IsNotNull(folder);
		}

		[DisplayName("*  [Convert DTE ProjectItem to Clide's IItemNode](" + baseUrl + "233)")]
		[HostType("VS IDE")]
		[TestMethod]
		public void how_to_convert_DTE_ProjectItem_to_IItemNode()
		{
			// Say you got a DTE project item somehow.
			EnvDTE.ProjectItem dteItem = this.DteLibrary.ProjectItems.OfType<ProjectItem>().First(pi => pi.Name == "Class1.cs");

			IItemNode itemNode = dteItem.Adapt().AsItemNode();

			Assert.IsNotNull(itemNode);

			// Now we can use Clide's item node API, such as setting an 
			// arbitrary MSBuild item metadata property using dynamic syntax.
			// This example sets the custom tool for the item to be MSBuild:Compile.
			itemNode.Properties.Generator = "MSBuild:Compile";

			Assert.AreEqual("MSBuild:Compile", (string)itemNode.Properties.Generator);
		}

		[DisplayName("*  [Convert DTE ProjectItem to MSBuild ProjectItem](" + baseUrl + "253)")]
		[HostType("VS IDE")]
		[TestMethod]
		public void how_to_convert_DTE_ProjectItem_to_MSBuild_ProjectItem()
		{
			// Say you got a DTE project item somehow.
			EnvDTE.ProjectItem dteItem = this.DteLibrary.ProjectItems.OfType<ProjectItem>().First(pi => pi.Name == "Class1.cs");

			Microsoft.Build.Evaluation.ProjectItem item = dteItem.Adapt().AsMsBuildItem();

			Assert.IsNotNull(item);

			// Now use MSBuild to set/get custom metadata on the item
			item.SetMetadataValue("Identifier", "Foo");
			string identifier = item.GetMetadataValue("Identifier");
			
			Assert.AreEqual("Foo", identifier);
		}


		[TestInitialize]
		public override void TestInitialize()
		{
			base.TestInitialize();

			base.OpenSolution("SampleSolution\\SampleSolution.sln");

			Solution = DevEnv.Get(ServiceProvider).SolutionExplorer().Solution;
			Assert.IsNotNull(Solution);

			LibraryNode = Solution.FindProjects(p => p.DisplayName == "ClassLibrary").First();
			Assert.IsNotNull(LibraryNode);

			DteLibrary = LibraryNode.As<Project>();
			IVsLibrary = LibraryNode.As<IVsProject>();
			VsLangLibrary = LibraryNode.As<VSLangProj.VSProject>();

			Assert.IsNotNull(DteLibrary);
			Assert.IsNotNull(IVsLibrary);
			Assert.IsNotNull(VsLangLibrary);
		}

		public ISolutionNode Solution { get; private set; }
		public IProjectNode LibraryNode { get; private set; }
		public EnvDTE.Project DteLibrary { get; private set; }
		public IVsProject IVsLibrary { get; private set; }
		public VSLangProj.VSProject VsLangLibrary { get; private set; }
	}
}
