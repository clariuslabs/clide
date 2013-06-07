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

namespace Clide.Solution
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [TestClass]
    public class ItemDataSpec : VsHostedSpec
    {
        [TestMethod]
        public void WhenSettingItemData_ThenCanRetrieveIt()
        {
            this.OpenSolution("SampleSolution\\SampleSolution.sln");

            var explorer = base.ServiceLocator.GetInstance<ISolutionExplorer>();

            explorer.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .Where(node => node.DisplayName == "TextFile1.txt")
                .ToList()
                .ForEach(node => Console.WriteLine(node.GetType().FullName));

            var item = explorer.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<IItemNode>()
                .FirstOrDefault(node => node.DisplayName == "TextFile1.txt");

            item.Properties.Foo = "bar";
            Assert.AreEqual("bar", (string)item.Properties.Foo);

            explorer.Solution.Save();

            this.CloseSolution();
            this.OpenSolution("SampleSolution\\SampleSolution.sln");

            item = explorer.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<IItemNode>()
                .FirstOrDefault(node => node.DisplayName == "TextFile1.txt");

            Assert.AreEqual("bar", (string)item.Properties.Foo);
        }
    }
}
