using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
namespace Clide
{

    public class SolutionExtensionsSpec
    {
        ISolutionExplorer explorer;

        public SolutionExtensionsSpec()
        {
            var solution = new FakeSolution
            {
                Nodes =
                    {
                        new FakeSolutionFolder("Solution Items")
                        {
                            Nodes =
                            {
                                new FakeSolutionItem("Readme.md"),
                            }
                        },
                        new FakeSolutionFolder("CSharp")
                        {
                            Nodes =
                            {
                                new FakeProject("CsConsole")
                                {
                                    Nodes =
                                    {
                                        new FakeItem("Class1.cs"),
                                    }
                                }
                            }
                        },
                        new FakeSolutionFolder("VB")
                        {
                            Nodes =
                            {
                                new FakeProject("VbConsole")
                                {
                                    Nodes =
                                    {
                                        new FakeItem("Class1.vb"),
                                    }
                                }
                            }
                        },
                    }
            };

            explorer = new FakeSolutionExplorer
            {
                Solution = Awaitable.Create<ISolutionNode>(async () => solution)
            };
        }

        [Fact]
        public async Task when_getting_relative_path_from_class_to_solution_folder_then_makes_relative_path()
        {
            var c = (await explorer.Solution).Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<IItemNode>().First(i => i.Name == "Class1.cs");

            var f = (await explorer.Solution).Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<FakeSolutionFolder>().First(i => i.Name == "CSharp");

            var path = c.RelativePathTo(f);

            Assert.Equal("CsConsole\\Class1.cs", path);
        }

        [Fact]
        public async Task when_ancestor_node_is_not_ancestor_then_throws_arguement_exception()
        {
            var c = (await explorer.Solution).Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<IItemNode>().First(i => i.Name == "Class1.cs");

            var f = (await explorer.Solution).Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<FakeSolutionFolder>().First(i => i.Name == "VB");

            Assert.Throws<ArgumentException>(() => c.RelativePathTo(f));
        }

        [Fact]
        public async Task when_getting_relative_path_from_solution_item_to_solution_then_makes_relative_path()
        {
            var c = (await explorer.Solution).Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<ISolutionItemNode>().First();

            var path = c.RelativePathTo((await explorer.Solution));

            Assert.Equal("Solution Items\\Readme.md", path);
        }

        [Fact]
        public async Task when_getting_relative_path_from_project_to_solution_then_makes_relative_path()
        {
            var c = (await explorer.Solution).Nodes.Traverse(TraverseKind.BreadthFirst, node => node.Nodes)
                .OfType<IProjectNode>().First();

            var path = c.RelativePathTo((await explorer.Solution));

            Assert.Equal("CSharp\\CsConsole", path);
        }

        [Fact]
        public async Task when_finding_all_projects_then_gets_all()
        {
            var projects = (await explorer.Solution).FindProjects().ToList();

            Assert.Equal(2, projects.Count);
        }

        [Fact]
        public async Task when_finding_all_projects_with_filter_then_gets_all_matches()
        {
            var projects = (await explorer.Solution).FindProjects(p => p.Name.Contains("Console")).ToList();

            Assert.Equal(2, projects.Count);
        }

        [Fact]
        public async Task when_finding_project_then_can_filter_by_name()
        {
            var project = (await explorer.Solution).FindProject(p => p.Name == "CsConsole");

            Assert.NotNull(project);
        }
    }
}