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
	using Microsoft.Build.Evaluation;

	[TestClass]
	[DisplayName(
@"## Convert between APIs
")]
	public partial class AdaptProject : VsHostedSpec
	{
		const string baseUrl = "Adapters/AdaptProject.cs#L";

		[DisplayName("*  [Convert IVsSolution to Clide's ISolutionNode](" + baseUrl + "34)")]
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

		[DisplayName("*  [Convert IVsSolution to DTE Solution](" + baseUrl + "50)")]
		[HostType("VS IDE")]
		[TestMethod]
		public void how_to_convert_IVsSolution_DTE_Solution()
		{
			// Say you have an IVsSolution:
			var vsSolution = (IVsSolution)ServiceProvider.GetService(typeof(SVsSolution));

			EnvDTE.Solution dteSolution = vsSolution.Adapt().AsDteSolution();

			Assert.IsNotNull(dteSolution);
		}

		[DisplayName("*  [Convert DTE Solution to IVsSolution](" + baseUrl + "63)")]
		[HostType("VS IDE")]
		[TestMethod]
		public void how_to_convert_DTE_Solution_to_IVsSolution()
		{
			// Say you have a DTE Solution:
			EnvDTE.Solution dteSolution = Dte.Solution;

			var vsSolution = dteSolution.Adapt().AsVsSolution();

			Assert.IsNotNull(dteSolution);
		}

		[DisplayName("*  [Convert DTE Solution to Clide's ISolutionNode](" + baseUrl + "76)")]
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

		[DisplayName("*  [Convert DTE Project to MSBuild Project](" + baseUrl + "92)")]
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

		[DisplayName("*  [Convert DTE Project to IVsProject](" + baseUrl + "107)")]
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

		[DisplayName("*  [Convert DTE Project to VSLangProj.VSProject (C# or VB project)](" + baseUrl + "126)")]
		[HostType("VS IDE")]
		[TestMethod]
		public void how_to_convert_DTE_project_to_VSProject()
		{
			// Say you got a DTE project somehow.
			EnvDTE.Project dteProject = this.DteLibrary;

			VSLangProj.VSProject vsProject = dteProject.Adapt().AsVsLangProject();

			Assert.IsNotNull(vsProject);

			// Use the project, for example, to create the Web References folder
			EnvDTE.ProjectItem folder = vsProject.CreateWebReferencesFolder();
			Assert.IsNotNull(folder);
		}

		[DisplayName("*  [Convert DTE Project to Clide's IProjectNode](" + baseUrl + "143)")]
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

		[DisplayName("*  [Convert IVsProject to Clide's IProjectNode](" + baseUrl + "167)")]
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

		[DisplayName("*  [Convert IVsProject to DTE Project](" + baseUrl + "185)")]
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

		[DisplayName("*  [Convert IVsProject to MSBuild Project](" + baseUrl + "201)")]
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

		[DisplayName("*  [Convert IVsProject to VSLangProj.VSProject (C# or VB project)](" + baseUrl + "217)")]
		[HostType("VS IDE")]
		[TestMethod]
		public void how_to_convert_IVsProject_to_VSProject()
		{
			// Say you got an IVsProject somehow.
			IVsProject vsProject = this.IVsLibrary;

			VSLangProj.VSProject langProject = vsProject.Adapt().AsVsLangProject();

			Assert.IsNotNull(langProject);

			// Use the project, for example, to create the Web References folder
			EnvDTE.ProjectItem folder = langProject.CreateWebReferencesFolder();
			Assert.IsNotNull(folder);
		}

		[DisplayName("*  [Convert DTE ProjectItem to Clide's IItemNode](" + baseUrl + "234)")]
		[HostType("VS IDE")]
		[TestMethod]
		public void how_to_convert_DTE_ProjectItem_to_IItemNode()
		{
			// Say you got a DTE project item somehow.
			EnvDTE.ProjectItem dteItem = this.DteLibrary.ProjectItems.OfType<EnvDTE.ProjectItem>().First(pi => pi.Name == "Class1.cs");

			IItemNode itemNode = dteItem.Adapt().AsItemNode();

			Assert.IsNotNull(itemNode);

			// Now we can use Clide's item node API, such as setting an 
			// arbitrary MSBuild item metadata property using dynamic syntax.
			// This example sets the custom tool for the item to be MSBuild:Compile.
			itemNode.Properties.Generator = "MSBuild:Compile";

			Assert.AreEqual("MSBuild:Compile", (string)itemNode.Properties.Generator);
		}

		[DisplayName("*  [Convert DTE ProjectItem to MSBuild ProjectItem](" + baseUrl + "254)")]
		[HostType("VS IDE")]
		[TestMethod]
		public void how_to_convert_DTE_ProjectItem_to_MSBuild_ProjectItem()
		{
			// Say you got a DTE project item somehow.
			EnvDTE.ProjectItem dteItem = this.DteLibrary.ProjectItems.OfType<EnvDTE.ProjectItem>().First(pi => pi.Name == "Class1.cs");

			Microsoft.Build.Evaluation.ProjectItem item = dteItem.Adapt().AsMsBuildItem();

			Assert.IsNotNull(item);

			// Now use MSBuild to set/get custom metadata on the item
			item.SetMetadataValue("Identifier", "Foo");
			string identifier = item.GetMetadataValue("Identifier");
			
			Assert.AreEqual("Foo", identifier);
		}

		[DisplayName("*  [Convert MSBuild Project to DTE Project](" + baseUrl + "274)")]
		[HostType("VS IDE")]
		[TestMethod]
		public void how_to_convert_MSBuild_Project_to_DTE_Project()
		{
			// Say you got an MSBuild Project somehow.
			Microsoft.Build.Evaluation.Project project = this.MsBuildLibrary;

			EnvDTE.Project dteProject = project.Adapt().AsDteProject();

			Assert.IsNotNull(dteProject);

			// Use the DTE project API, such as to save the project:
			dteProject.Save();
		}

		[DisplayName("*  [Convert MSBuild Project to Clide's IProjectNode](" + baseUrl + "291)")]
		[HostType("VS IDE")]
		[TestMethod]
		public void how_to_convert_MSBuild_Project_to_IProjectNode()
		{
			// Say you got an MSBuild Project somehow.
			Microsoft.Build.Evaluation.Project project = this.MsBuildLibrary;

			IProjectNode projectNode = project.Adapt().AsProjectNode();

			Assert.IsNotNull(projectNode);

			// Now we can use Clide's project node API, such as accessing 
			// the MSBuild properties using dynamic syntax.
			string assemblyName = projectNode.Properties.AssemblyName;

			Assert.AreEqual("ClassLibrary", assemblyName);

			// Or a property for a specific configuration
			string debugType = projectNode.PropertiesFor("Debug|AnyCPU").DebugType;

			Assert.AreEqual("full", debugType);
		}

		[DisplayName("*  [Convert MSBuild Project to IVsProject](" + baseUrl + "313)")]
		[HostType("VS IDE")]
		[TestMethod]
		public void how_to_convert_MSBuild_Project_to_IVsProject()
		{
			// Say you got an MSBuild Project somehow.
			Microsoft.Build.Evaluation.Project project = this.MsBuildLibrary;

			IVsProject vsProject = project.Adapt().AsVsProject();

			Assert.IsNotNull(vsProject);

			// Use the VS project to open an item in a specific view, the designer, for example
			uint itemId = 0; // Get the item ID to open somehow, see other how-tos.
			IVsWindowFrame frame;
			Guid viewId = VSConstants.LOGVIEWID.Designer_guid;
			vsProject.OpenItem(itemId, ref viewId, IntPtr.Zero, out frame);
		}

		[DisplayName("*  [Convert MSBuild Project to VSLangProj.VSProject (C# or VB project)](" + baseUrl + "332)")]
		[HostType("VS IDE")]
		[TestMethod]
		public void how_to_convert_MSBuild_Project_to_VsLangProject()
		{
			// Say you got an MSBuild Project somehow.
			Microsoft.Build.Evaluation.Project project = this.MsBuildLibrary;

			VSLangProj.VSProject vsProject = project.Adapt().AsVsLangProject();

			Assert.IsNotNull(vsProject);

			// Use the project, for example, to create the Web References folder
			EnvDTE.ProjectItem folder = vsProject.CreateWebReferencesFolder();
			Assert.IsNotNull(folder);
		}

		[DisplayName("*  [Convert MSBuild ProjectItem to DTE ProjectItem](" + baseUrl + "349)")]
		[HostType("VS IDE")]
		[TestMethod]
		public void how_to_convert_MSBuild_ProjectItem_to_DTE_ProjectItem()
		{
			// Say you got an MSBuild ProjectItem somehow.
			Microsoft.Build.Evaluation.ProjectItem item = this.MsBuildLibrary.Items.First(pi => pi.UnevaluatedInclude == "Class1.cs");

			EnvDTE.ProjectItem dteItem = item.Adapt().AsDteProjectItem();

			Assert.IsNotNull(dteItem);

			// Now use DTE to delete the item, for example
			dteItem.Delete();
		}

		[DisplayName("*  [Convert MSBuild ProjectItem to Clide's IItemNode](" + baseUrl + "365)")]
		[HostType("VS IDE")]
		[TestMethod]
		public void how_to_convert_MSBuild_ProjectItem_to_IItemNode()
		{
			// Say you got an MSBuild ProjectItem somehow.
			Microsoft.Build.Evaluation.ProjectItem item = this.MsBuildLibrary.Items.First(pi => pi.UnevaluatedInclude == "Class1.cs");

			IItemNode itemNode = item.Adapt().AsItemNode();

			Assert.IsNotNull(itemNode);

			// Now use item node to expand it (show its nested items)
			itemNode.Expand();
		}

		[DisplayName("*  [Convert MSBuild ProjectItem to VSLangProj.VSProjectItem (C# or VB item)](" + baseUrl + "381)")]
		[HostType("VS IDE")]
		[TestMethod]
		public void how_to_convert_MSBuild_ProjectItem_to_VSProjectItem()
		{
			// Say you got an MSBuild ProjectItem somehow.
			Microsoft.Build.Evaluation.ProjectItem item = this.MsBuildLibrary.Items.First(pi => pi.UnevaluatedInclude == "Class1.cs");

			VSLangProj.VSProjectItem vsItem = item.Adapt().AsVsLangItem();

			Assert.IsNotNull(vsItem);

			// Now use item to force its custom tool to run.
			vsItem.RunCustomTool();
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

			DteLibrary = LibraryNode.As<EnvDTE.Project>();
			IVsLibrary = LibraryNode.As<IVsProject>();
			VsLangLibrary = LibraryNode.As<VSLangProj.VSProject>();
			MsBuildLibrary = LibraryNode.As<Microsoft.Build.Evaluation.Project>();

			Assert.IsNotNull(DteLibrary);
			Assert.IsNotNull(IVsLibrary);
			Assert.IsNotNull(VsLangLibrary);
			Assert.IsNotNull(MsBuildLibrary);
		}

		public ISolutionNode Solution { get; private set; }
		public IProjectNode LibraryNode { get; private set; }
		public EnvDTE.Project DteLibrary { get; private set; }
		public IVsProject IVsLibrary { get; private set; }
		public Microsoft.Build.Evaluation.Project MsBuildLibrary { get; private set; }
		public VSLangProj.VSProject VsLangLibrary { get; private set; }
	}
}
