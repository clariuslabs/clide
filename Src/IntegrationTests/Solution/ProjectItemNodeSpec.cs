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
	using Clide.VisualStudio;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using System.IO;
	using System.Linq;
	using System.Xml.Linq;

	[TestClass]
	public class ProjectItemNodeSpec : VsHostedSpec
	{
		internal static readonly IAssertion Assert = new Assertion();

		[HostType("VS IDE")]
		[TestMethod]
		public void WhenComparingItemsInProject_ThenCanCheckForEquality()
		{
			base.OpenSolution("SampleSolution\\SampleSolution.sln");

			var explorer = base.ServiceLocator.GetInstance<ISolutionExplorer>();

			var node1 = explorer.Solution.Traverse().OfType<IItemNode>().First(i => i.DisplayName == "Class1.cs");
			var node2 = explorer.Solution.Traverse().OfType<IItemNode>().First(i => i.DisplayName == "Class1.cs");

			Assert.Equal(node1.OwningProject, node2.OwningProject, "Owning projects aren't equal.");
			Assert.Equal(node1.As<VsHierarchyItem>().ItemId, node2.As<VsHierarchyItem>().ItemId, "ItemIds aren't equal.");

			Assert.Equal(node1, node2, "Items aren't equal.");
		}
	}
}