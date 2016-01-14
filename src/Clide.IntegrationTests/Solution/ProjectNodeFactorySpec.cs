using Xunit;

namespace Clide.Solution.Project
{

	[Collection ("OpenSolution")]
	[Vsix (MinimumVisualStudioVersion = VisualStudioVersion.VS2013)]
	public class ProjectNodeFactorySharedSpec : NodeFactorySpec<ProjectNodeFactory>
	{
		[InlineData ("Shared\\CppShared", "")]
		[InlineData ("Shared\\CsShared", "")]
		[InlineData ("Shared\\VbShared", "14.0")]
		[VsixTheory]
		public override void when_item_is_supported_then_factory_supports_it (string relativePath, string minimumVersion)
		{
			base.when_item_is_supported_then_factory_supports_it (relativePath, minimumVersion);
		}

		[InlineData ("Shared\\CppShared\\SharedSource.cpp", "")]
		[InlineData ("Shared\\CsShared\\SharedClass1.cs", "")]
		[InlineData ("Shared\\VbShared\\SharedClass1.vb", "14.0")]
		[VsixTheory]
		public override void when_item_is_not_supported_then_factory_returns_false_and_create_returns_null (string relativePath, string minimumVersion)
		{
			base.when_item_is_not_supported_then_factory_returns_false_and_create_returns_null (relativePath, minimumVersion);
		}
	}

	[Collection ("OpenSolution11")]
	public class ProjectNodeFactorySpec : NodeFactorySpec<ProjectNodeFactory>
	{
		[InlineData ("Native\\CppLibrary")]
		[InlineData ("Native\\CsLibrary")]
		[InlineData ("Native\\VbLibrary")]
		[InlineData ("Native\\FsLibrary")]
		[InlineData ("PclLibrary")]
		[VsixTheory]
		public override void when_item_is_supported_then_factory_supports_it (string relativePath)
		{
			base.when_item_is_supported_then_factory_supports_it (relativePath);
		}

		[InlineData ("Native\\CppLibrary\\References")]
		[InlineData ("Native\\CppLibrary\\References\\System")]
		[InlineData ("Native\\VbLibrary\\References")]
		[InlineData ("Native\\VbLibrary\\References\\System")]
		[VsixTheory (MinimumVisualStudioVersion = VisualStudioVersion.VS2015)]
		public void when_2015_item_is_not_supported_then_factory_returns_false_and_create_returns_null (string relativePath)
		{
			when_item_is_not_supported_then_factory_returns_false_and_create_returns_null (relativePath);
		}

		[InlineData ("")]
		[InlineData("Native")]
		[InlineData ("Native\\CppLibrary\\ReadMe.txt")]
		[InlineData ("Native\\CppLibrary\\CppFolder")]
		[InlineData ("Native\\CsLibrary\\Class1.cs")]
		[InlineData ("Native\\CsLibrary\\CsFolder")]
		[InlineData ("Native\\VbLibrary\\Class1.vb")]
		[InlineData ("Native\\VbLibrary\\VbFolder")]
		[InlineData ("Native\\FsLibrary\\Library1.fs")]
		[InlineData ("Native\\FsLibrary\\FsFolder")]
		[InlineData ("Native\\CppLibrary\\External Dependencies")]
		[InlineData ("Native\\CsLibrary\\References")]
		[InlineData ("Native\\CsLibrary\\References\\System")]
		[InlineData ("Native\\FsLibrary\\References")]
		[InlineData ("Native\\FsLibrary\\References\\System")]
		[InlineData ("PclLibrary\\References")]
		[InlineData ("PclLibrary\\References\\.NET")]
		[InlineData ("Solution Items")]
		[InlineData ("Solution Items\\SolutionItem.txt")]
		[VsixTheory]
		public override void when_item_is_not_supported_then_factory_returns_false_and_create_returns_null (string relativePath)
		{
			base.when_item_is_not_supported_then_factory_returns_false_and_create_returns_null (relativePath);
		}

		/*
        [HostType("VS IDE")]
        [TestMethod]
        public void WhenSolutionIsOpened_ThenReturnsProperHierarchy()
        {
            var folder1 = solution.Nodes.FirstOrDefault(n => n.DisplayName == "SolutionFolder1") as ISolutionFolderNode;
            Assert.NotNull(folder1);
            Assert.NotNull(folder1.OwningSolution);

            var vbLib = folder1.Nodes.FirstOrDefault(n => n.DisplayName == "VBClassLibrary") as IProjectNode;
            Assert.NotNull(vbLib);
            Assert.NotNull(vbLib.OwningSolution);

            var vbRefs = vbLib.Nodes.FirstOrDefault(n => n.DisplayName == "References");
            Assert.NotNull(vbRefs);
            Assert.True(vbRefs.IsHidden);
            Assert.NotNull(vbLib.OwningSolution);

            var folder2 = folder1.Nodes.FirstOrDefault(n => n.DisplayName == "SolutionFolder2") as ISolutionFolderNode;
            Assert.NotNull(folder2);

            var csLib = folder2.Nodes.FirstOrDefault(n => n.DisplayName == "ClassLibrary") as IProjectNode;
            Assert.NotNull(csLib);

            var nodes = csLib.Nodes.Select(n => n.DisplayName).ToList();

            var folder = csLib.Nodes.FirstOrDefault(n => n.DisplayName == "Folder") as IFolderNode;
            Assert.NotNull(folder.Nodes.FirstOrDefault(n => n.DisplayName == "TextFile1.txt") as IItemNode);
            Assert.NotNull(csLib.Nodes.FirstOrDefault(n => n.DisplayName == "Class1.cs") as IItemNode);
            Assert.NotNull(folder.OwningSolution);

            Assert.Equal("ClassLibrary", ((IItemNode)folder.Nodes.FirstOrDefault(n => n.DisplayName == "TextFile1.txt")).OwningProject.DisplayName);
            Assert.Equal("ClassLibrary", ((IItemNode)csLib.Nodes.FirstOrDefault(n => n.DisplayName == "Class1.cs")).OwningProject.DisplayName);

            var references = csLib.Nodes.FirstOrDefault(n => n.DisplayName == "References") as IReferencesNode;
            Assert.NotNull(references, "No References node was exposed in the tree.");
            Assert.False(references.IsHidden);

            Assert.NotEqual(0, references.Nodes.Count());

            Assert.Equal("ClassLibrary", ((IReferencesNode)csLib.Nodes.FirstOrDefault(n => n.DisplayName == "References")).OwningProject.DisplayName);

            Assert.NotNull(references.Nodes.First() as IReferenceNode);

            var item = solution
                .Nodes.First(node => node.DisplayName == "Solution Items")
                .Nodes.First(node => node.DisplayName == "SolutionItem.txt");

            Assert.True(item is ISolutionItemNode);
            Assert.NotNull(((ISolutionItemNode)item).OwningSolution);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenTraversingHiddenNode_ThenItIsHiddenAndInvisible()
        {
            var project = solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<IProjectNode>()
                .First(n => n.DisplayName == "VBClassLibrary");

            // In VB, References node is hidden, but exists in the hierarchy.
            var references = project.Nodes.Single(node => node.DisplayName == "References");

            Assert.True(references.IsHidden);
            Assert.False(references.IsVisible);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenTraversingHiddenDescendentNode_ThenItIsHiddenAndInvisible()
        {
            var project = solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<IProjectNode>()
                .First(n => n.DisplayName == "VBClassLibrary");

            // In VB, References node is hidden, but exists in the hierarchy.
            var references = project.Nodes.OfType<IReferencesNode>().Single();
            var reference = references.Nodes.Single(node => node.DisplayName == "System.Xml.Linq");

            Assert.True(reference.IsHidden);
            Assert.False(reference.IsVisible);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenItemFound_ThenOwningProjectIsValid()
        {
            var item = solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<IItemNode>()
                .FirstOrDefault(node => node.DisplayName == "TextFile1.txt");

            Assert.NotNull(item);
            Assert.Equal("ClassLibrary", item.OwningProject.DisplayName);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenFolderFound_ThenOwningProjectIsValid()
        {
            var target = solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<IFolderNode>()
                .FirstOrDefault(node => node.DisplayName == "Folder");

            Assert.NotNull(target);
            Assert.Equal("ClassLibrary", target.OwningProject.DisplayName);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenReferencesNode_ThenOwningProjectIsValid()
        {
            var target = solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<IReferencesNode>()
                .FirstOrDefault(node => !node.IsHidden);

            Assert.NotNull(target);
            Assert.Equal("ClassLibrary", target.OwningProject.DisplayName);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenReferenceFound_ThenOwningProjectIsValid()
        {
            var target = solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<IReferenceNode>()
                .FirstOrDefault(node => !node.IsHidden && node.DisplayName == "System.Xml.Linq");

            Assert.NotNull(target);
            Assert.Equal("ClassLibrary", target.OwningProject.DisplayName);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenSolutionItemFound_ThenOwningSolutionFolderIsValid()
        {
            solution.Nodes
                .Traverse(TraverseKind.BreadthFirst, node => node.Nodes)
                .ToList()
                .ForEach(node => Console.WriteLine("{0} ({1})", node.DisplayName, node.GetType().Name));

            var target = solution.Nodes
                .OfType<ISolutionFolderNode>()
                .First()
                .Nodes
                .OfType<ISolutionItemNode>()
                .FirstOrDefault();

            Assert.NotNull(target);
            Assert.Equal("Solution Items", target.OwningSolutionFolder.DisplayName);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenProjectIsSelected_ThenCanRetrieveSelectedNode()
        {
            var project = solution.FindProjects().First();
            project.Select();

            var selection = solution.SelectedNodes.OfType<IProjectNode>().First();

            Assert.Equal(project.PhysicalPath, selection.PhysicalPath);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenMultipleProjectsAreSelected_ThenCanRetrieveAllSelectedNodes()
        {
            var allProjects = solution.FindProjects().ToList();
            allProjects.ForEach(x => x.Select(true));

            var selection = solution.SelectedNodes.OfType<IProjectNode>().ToList();

            Assert.Equal(allProjects.Count, selection.Count);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenSelectionIsSolution_ThenActiveProjectIsNull()
        {
            solution.Select();

            var active = solution.ActiveProject;

            Assert.Null(active);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenNoSolutionIsOpened_ThenActiveProjectIsNull()
        {
            CloseSolution();

            var active = solution.ActiveProject;

            Assert.Null(active);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenSingleProjectIsSelected_ThenActiveProjectIsNotNull()
        {
            var project = solution.FindProjects().First();
            project.Select();

            var active = solution.ActiveProject;

            Assert.NotNull(active);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenSingleItemIsSelected_ThenOwningProjectIsActive()
        {
            var item = solution.Traverse().OfType<IItemNode>().First(i => i.DisplayName.EndsWith(".cs"));
            item.Select();

            var active = solution.ActiveProject;

            Assert.NotNull(active);
            Assert.Equal(active.DisplayName, item.OwningProject.DisplayName);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenMultipleItemsInSingleHierarchyAreSelected_ThenOwningProjectIsActive()
        {
            var classes = solution.FindProjects().First()
                .Traverse().OfType<IItemNode>()
                .Where(i => i.DisplayName.EndsWith(".cs"))
                .ToList();
                
            // First clear whatever other selection there it for another project
            classes[0].Select(false);
            classes.ForEach(i => i.Select(true));

            var active = solution.ActiveProject;

            Assert.NotNull(active);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenMultipleItemsInDifferentHierarchiesAreSelected_ThenActiveProjectIsNull()
        {
            solution.FindProjects().First()
                .Traverse().OfType<IItemNode>()
                .Where(i => i.DisplayName.EndsWith(".cs"))
                .First().Select();

            solution.FindProjects()
                .SelectMany(p => p.Traverse())
                .Where(i => i.DisplayName == ("Class1.cs") || i.DisplayName == "Class1.vb")
                .OfType<IItemNode>()
                .AsParallel().ForAll(i => i.Select(true));

            var active = solution.ActiveProject;

            Assert.Null(active);
        }
 	*/
	}
}