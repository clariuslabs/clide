#region BSD License
/* 
Copyright (c) 2012, Clarius Consulting
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

namespace UnitTests.Solution.ISolutionExplorerNodeExtensionsSpec
{
	using Clide.Solution;
	using System;
	using System.Linq;
	using Xunit;

    public class given_a_solution
    {
        private ISolutionExplorer explorer;

        public given_a_solution()
        {
            explorer = new FakeSolutionExplorer
            {
                Solution = new FakeSolution
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
                }
            };

        }

        [Fact]
        public void when_getting_relative_path_from_class_to_solution_folder_then_makes_relative_path()
        {
			var c = explorer.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
				.OfType<IItemNode>().First(i => i.DisplayName == "Class1.cs");

			var f = explorer.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
				.OfType<FakeSolutionFolder>().First(i => i.DisplayName == "CSharp");

			var path = c.RelativePathTo(f);

			Assert.Equal("CsConsole\\Class1.cs", path);
        }

        [Fact]
        public void when_ancestor_node_is_not_ancestor_then_throws_arguement_exception()
        {
			var c = explorer.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
				.OfType<IItemNode>().First(i => i.DisplayName == "Class1.cs");

			var f = explorer.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
				.OfType<FakeSolutionFolder>().First(i => i.DisplayName == "VB");

			Assert.Throws<ArgumentException>(() => c.RelativePathTo(f));
        }

        [Fact]
        public void when_getting_relative_path_from_solution_item_to_solution_then_makes_relative_path()
        {
			var c = explorer.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
				.OfType<ISolutionItemNode>().First();

			var path = c.RelativePathTo(explorer.Solution);

			Assert.Equal("Solution Items\\Readme.md", path);
        }

        [Fact]
        public void when_getting_relative_path_from_project_to_solution_then_makes_relative_path()
        {
			var c = explorer.Solution.Nodes.Traverse(TraverseKind.BreadthFirst, node => node.Nodes)
				.OfType<IProjectNode>().First();

			var path = c.RelativePathTo(explorer.Solution);

			Assert.Equal("CSharp\\CsConsole", path);
        }

    }
}