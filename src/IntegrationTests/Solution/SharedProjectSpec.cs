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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clide.Solution
{
	[TestClass]
	public class SharedProjectSpec : VsHostedSpec
	{
		internal static readonly IAssertion Assert = new Assertion();

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenComparingSharedProjects_ThenCanCheckForEquality()
        {
			using (AutoCloseDialog("Developer License"))
			{
				base.OpenSolution("SampleShared\\SampleShared.sln");
			}

            var explorer = base.ServiceLocator.GetInstance<ISolutionExplorer>();

            var prj1 = new ITreeNode[] {explorer.Solution }.Traverse(TraverseKind.BreadthFirst, node => node.Nodes)
                .OfType<IProjectNode>()
                .FirstOrDefault(node => node.DisplayName == "Shared");

            var prj2 = new ITreeNode[] {explorer.Solution }.Traverse(TraverseKind.BreadthFirst, node => node.Nodes)
                .OfType<IProjectNode>()
                .FirstOrDefault(node => node.DisplayName == "Shared");

			Assert.Equal(prj1, prj2);
        }

        [HostType("VS IDE")]
        [TestMethod]
        public void WhenComparingItemsInSharedProjects_ThenCanCheckForEquality()
        {
			using (AutoCloseDialog("Developer License"))
			{
				base.OpenSolution("SampleShared\\SampleShared.sln");
			}

            var explorer = base.ServiceLocator.GetInstance<ISolutionExplorer>();

			var node1 = explorer.Solution.Traverse().OfType<IItemNode>().First(i => i.DisplayName == "Foo.cs");
			var node2 = explorer.Solution.Traverse().OfType<IItemNode>().First(i => i.DisplayName == "Foo.cs");

			Assert.Equal(node1, node2);
        }

		[HostType("VS IDE")]
		[TestMethod]
		public void WhenGettingLogicalPathOfItem_ThenSucceedsForSharedProject()
		{
			using (AutoCloseDialog("Developer License"))
			{
				base.OpenSolution("SampleShared\\SampleShared.sln");
			}

			var explorer = base.ServiceLocator.GetInstance<ISolutionExplorer>();

			var node = explorer.Solution.Traverse().OfType<IItemNode>().First(i => i.DisplayName == "Foo.cs");

			Assert.NotNull(node);

			Assert.Equal(node.OwningProject, node.OwningProject, "Retrieving owning project twice didn't return the same project.");
			Assert.Equal(node.OwningProject, node.OwningProject, "Retrieving owning project twice didn't return the same project.");

			var project = node.OwningProject;
			var ancestors = node.Ancestors().ToList();

			Assert.True(ancestors.Contains(project), "Ancestors doesn't contain owning project.");

			Assert.Equal("Folder\\Foo.cs", node.LogicalPath);
		}
	}
}
