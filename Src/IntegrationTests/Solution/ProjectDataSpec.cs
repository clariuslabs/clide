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
    using Microsoft.Build.Evaluation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Dynamic;
    using System.IO;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;

    [TestClass]
    public class ProjectDataSpec : VsHostedSpec
    {
        internal static readonly IAssertion Assert = new Assertion();

        [TestMethod]
        public void WhenRetrievingPropertyNames_ThenFindsDteAndMsBuild()
        {
            var explorer = base.ServiceLocator.GetInstance<ISolutionExplorer>();
            this.OpenSolution("SampleSolution\\SampleSolution.sln");

            var project = explorer.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<ProjectNode>()
                .First(node => node.DisplayName == "ClassLibrary");

            var names = ((DynamicObject)project.Properties).GetDynamicMemberNames().ToList();
            Console.WriteLine(string.Join(Environment.NewLine, names));

            Assert.True(names.Contains("OfflineURL"));
            Assert.True(names.Contains("StartupObject"));
            Assert.True(names.Contains("BuildingInsideVisualStudio"));
        }

        [TestMethod]
        public void WhenSettingDteProperty_ThenCanRetrieveIt()
        {
            var explorer = base.ServiceLocator.GetInstance<ISolutionExplorer>();
            this.OpenSolution("SampleSolution\\SampleSolution.sln");

            var project = explorer.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<ProjectNode>()
                .First(node => node.DisplayName == "ClassLibrary");

            project.Properties.StartupObject = "ClassLibrary.Program";

            project.Save();
            this.CloseSolution();

            this.OpenSolution("SampleSolution\\SampleSolution.sln");

            project = explorer.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<ProjectNode>()
                .First(node => node.DisplayName == "ClassLibrary");

            Assert.Equal("ClassLibrary.Program", (string)project.Properties.StartupObject);
        }

        [TestMethod]
        public void WhenSettingExistingMsBuildProperty_ThenCanRetrieveIt()
        {
            var explorer = base.ServiceLocator.GetInstance<ISolutionExplorer>();
            this.OpenSolution("SampleSolution\\SampleSolution.sln");

            var project = explorer.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<ProjectNode>()
                .First(node => node.DisplayName == "ClassLibrary");

            project.Properties.FileAlignment = "256";

            project.Save();
            this.CloseSolution();

            this.OpenSolution("SampleSolution\\SampleSolution.sln");

            project = explorer.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<ProjectNode>()
                .First(node => node.DisplayName == "ClassLibrary");

            Assert.Equal("256", (string)project.Properties.FileAlignment);
        }

        [TestMethod]
        public void WhenSettingNewMsBuildProperty_ThenCanRetrieveIt()
        {
            var explorer = base.ServiceLocator.GetInstance<ISolutionExplorer>();
            this.OpenSolution("SampleSolution\\SampleSolution.sln");

            var project = explorer.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<ProjectNode>()
                .First(node => node.DisplayName == "ClassLibrary");

            project.Properties.Author = "kzu";

            project.Save();
            this.CloseSolution();

            this.OpenSolution("SampleSolution\\SampleSolution.sln");

            project = explorer.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<ProjectNode>()
                .First(node => node.DisplayName == "ClassLibrary");

            Assert.Equal("kzu", (string)project.Properties.Author);
            File.WriteAllText("C:\\Temp\\project.txt", File.ReadAllText(project.PhysicalPath));
        }

        [TestMethod]
        public void WhenSettingExistingMsBuildConfiguration_ThenCanRetrieveIt()
        {
            var explorer = base.ServiceLocator.GetInstance<ISolutionExplorer>();
            this.OpenSolution("SampleSolution\\SampleSolution.sln");

            var project = explorer.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<ProjectNode>()
                .First(node => node.DisplayName == "ClassLibrary");

            project.PropertiesFor("Debug|AnyCPU").WarningLevel = "5";

            project.Save();
            this.CloseSolution();

            this.OpenSolution("SampleSolution\\SampleSolution.sln");

            project = explorer.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<ProjectNode>()
                .First(node => node.DisplayName == "ClassLibrary");

            Assert.Equal("5", (string)project.PropertiesFor("Debug|AnyCPU").WarningLevel);
        }

        [TestMethod]
        public void WhenGettingAllProperties_ThenSucceeds()
        {
            this.OpenSolution("SampleSolution\\SampleSolution.sln");

            var explorer = base.ServiceLocator.GetInstance<ISolutionExplorer>();

            var project = explorer.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<ProjectNode>()
                .FirstOrDefault(node => node.DisplayName == "ClassLibrary");

            Assert.NotNull(project);

            var vsBuild = (IVsBuildPropertyStorage)project.HierarchyNode.VsHierarchy;
            ErrorHandler.ThrowOnFailure(vsBuild.SetPropertyValue(
                "RootNamespace", "", (uint)_PersistStorageType.PST_PROJECT_FILE, "MyLibrary"));

            ErrorHandler.ThrowOnFailure(vsBuild.SetPropertyValue(
                "WarningLevel", "Debug|AnyCPU", (uint)_PersistStorageType.PST_PROJECT_FILE, "5"));
            ErrorHandler.ThrowOnFailure(vsBuild.SetPropertyValue(
                "WarningLevel", "Release|AnyCPU", (uint)_PersistStorageType.PST_PROJECT_FILE, "1"));

            Console.WriteLine(project.PhysicalPath);
            project.Save();
            File.WriteAllText("C:\\Temp\\project.txt", File.ReadAllText(project.PhysicalPath));

            var msb = project.As<Project>();

            var writer = new StringWriter();

            writer.WriteLine("----- AllEvaluatedProperties -----");
            foreach (var prop in msb.AllEvaluatedProperties)
            {
                writer.WriteLine("{0}={1}", prop.Name, prop.EvaluatedValue);
                //writer.WriteLine("{0}={1}", prop.Name, prop.Xml == null ? prop.EvaluatedValue : prop.Xml.AsDynamicReflection().XmlElement.OuterXml);
            }

            writer.WriteLine();
            writer.WriteLine("----- GlobalProperties -----");
            foreach (var prop in msb.GlobalProperties)
            {
                writer.WriteLine("{0}={1}", prop.Key, prop.Value);
            }

            writer.WriteLine();
            writer.WriteLine("----- Properties -----");
            foreach (var prop in msb.Properties)
            {
                writer.WriteLine("{0}={1}", prop.Name, prop.EvaluatedValue);
                //writer.WriteLine("{0}={1}", prop.Name, prop.Xml == null ? prop.EvaluatedValue : prop.Xml.AsDynamicReflection().XmlElement.OuterXml);
            }

            writer.WriteLine();
            writer.WriteLine("----- ConditionedProperties -----");
            foreach (var prop in msb.ConditionedProperties)
            {
                writer.WriteLine("{0}={1}", prop.Key, string.Join("|", prop.Value));
            }

            // msb.GetProperty("foo").d

            File.WriteAllText("C:\\Temp\\properties.txt", writer.ToString());

            /*
             * 
            
            project.Properties.All.RootNamespace = "Foo";
            project.Properties.RootNamespace = "Foo";
            project.PropertiesFor("Debug|AnyCPU").RootNamespace = "Bar";
            
            project.Properties.ForCondition("$(BuildingInsideVisualStudio)").RootNamespace = "Foo";
            project.Properties.ForConfiguration("Debug|AnyCPU").RootNamespace = "Foo";

            project.Properties.RootNamespace = "Foo";
            project.PropertiesForCondition("$(BuildingInsideVisualStudio)").RootNamespace = "Foo";
            project.PropertiesForConfiguration("Debug|AnyCPU").RootNamespace = "Foo";

             * 
             */
            //project.Properties.AllConfigurations.Foo = "bar";
            //Assert.AreEqual("bar", project.Properties.AllConfigurations.Foo);

            //project.Properties.AllConfigurations.Foo = "bar";
            //Assert.AreEqual("bar", project.Properties.AllConfigurations.Foo);
        }

        [TestMethod]
        public void WhenGettingGlobalProperty_ThenRetrievesActiveConfigurationValue()
        {
            this.OpenSolution("SampleSolution\\SampleSolution.sln");

            var explorer = base.ServiceLocator.GetInstance<ISolutionExplorer>();

            var project = explorer.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .OfType<ProjectNode>()
                .FirstOrDefault(node => node.DisplayName == "ClassLibrary");

            var fileName = (string)project.Properties.TargetFileName;
            var outDir = (string)project.Properties.TargetDir;

            // Both properties are set via MSBuild for the current configuration.

            Assert.NotNull(fileName);
            Assert.NotNull(outDir);
            Assert.NotEqual("", fileName);
            Assert.NotEqual("", outDir);
        }
    }
}
