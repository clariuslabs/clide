namespace Clide
{
    using Clide.Solution;
    using EnvDTE;
    using Microsoft.CSharp.RuntimeBinder;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Dynamic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [TestClass]
    public class Misc : VsHostedSpec
    {
        [HostType("VS IDE")]
        [TestMethod]
        public void WhenRetrievingHierarchyItem_ThenAlwaysGetsSameInstance()
        {
            this.OpenSolution("SampleSolution\\SampleSolution.sln");
            var projectId = new Guid("{883173CE-9FDE-4436-96F5-128EA8A471BE}");

            var solution = this.ServiceProvider.GetService<SVsSolution, IVsSolution>();

            var solutionHashCode = solution.GetHashCode();

            IVsHierarchy hierarchy1;
            IVsHierarchy hierarchy2;

            ErrorHandler.ThrowOnFailure(solution.GetProjectOfGuid(ref projectId, out hierarchy1));
            ErrorHandler.ThrowOnFailure(solution.GetProjectOfGuid(ref projectId, out hierarchy2));

            Assert.IsNotNull(hierarchy1);
            Assert.IsNotNull(hierarchy2);

            Assert.AreSame(hierarchy1, hierarchy2);

            var hashCodes = new List<int>(new[] { hierarchy1.GetHashCode(), hierarchy2.GetHashCode() });

            this.CloseSolution();

            this.OpenSolution("SampleSolution\\SampleSolution.sln");

            solution = this.ServiceProvider.GetService<SVsSolution, IVsSolution>();

            Assert.AreEqual(solutionHashCode, solution.GetHashCode());

            ErrorHandler.ThrowOnFailure(solution.GetProjectOfGuid(ref projectId, out hierarchy2));

            Assert.AreNotSame(hierarchy1, hierarchy2);

            Assert.IsFalse(hashCodes.Contains(hierarchy2.GetHashCode()));
        }

        [TestMethod]
        public void WhenSettingData_ThenSavesToProject()
        {
            this.OpenSolution("SampleSolution\\SampleSolution.sln");

            var explorer = base.Container.GetExportedValue<ISolutionExplorer>();

            var item = explorer.Solution.Nodes.Traverse(TraverseKind.DepthFirst, node => node.Nodes)
                .FirstOrDefault(node => node.DisplayName == "TextFile1.txt");

            Assert.IsNotNull(item);

            Debug.Fail("Attach");

            var tostring = item.ToString();

            dynamic dynItem = new VsItemDynamicProperties(item.As<IVsSolutionHierarchyNode>());

            dynItem.Foo = "bar";

            var names = ((DynamicObject)dynItem).GetDynamicMemberNames().ToList();

            names.ForEach(name => Console.WriteLine(name));

            Assert.AreEqual("bar", dynItem.Foo);
        }

        public class VsItemDynamicProperties : DynamicObject
        {
            private static readonly ITracer tracer = Tracer.Get<VsItemDynamicProperties>();

            IVsSolutionHierarchyNode node;

            public VsItemDynamicProperties(IVsSolutionHierarchyNode item)
            {
                this.node = item;
            }

            public override IEnumerable<string> GetDynamicMemberNames()
            {
                return GetPropertyNames();
            }

            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                return SetValue(binder.Name, value);
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                result = GetValue(binder.Name);
                return true;
            }

            public object GetValue(string name)
            {
                string value = null;

                var item = this.node.ExtensibilityObject as ProjectItem;
                if (item != null)
                {
                    Property property;
                    try
                    {
                        property = item.Properties.Item(name);
                    }
                    catch (ArgumentException)
                    {
                        property = null;
                    }

                    if (property != null)
                    {
                        return property.Value;
                    }
                }

                var storage = this.node as IVsBuildPropertyStorage;
                if (storage != null)
                {
                    storage.GetItemAttribute(this.node.ItemId, name, out value);
                }

                return value;
            }

            public bool SetValue(string name, object value)
            {
                if (value == null)
                    throw new NotSupportedException("Cannot set null value for custom MSBuild item properties.");

                var item = this.node.ExtensibilityObject as ProjectItem;
                if (item != null)
                {
                    Property property;
                    try
                    {
                        property = item.Properties.Item(name);
                    }
                    catch (ArgumentException)
                    {
                        property = null;
                    }

                    if (property != null)
                    {
                        try
                        {
                            property.Value = value;
                            return true;
                        }
                        catch
                        {
                            return false;
                        }
                    }
                }

                // Fallback to MSBuild item properties.
                var storage = this.node.VsHierarchy as IVsBuildPropertyStorage;
                if (storage != null)
                {
                    return ErrorHandler.Succeeded(
                        storage.SetItemAttribute(this.node.ItemId, name, value.ToString()));
                }

                return false;
            }

            private IEnumerable<string> GetPropertyNames()
            {
                try
                {
                    var names = new List<string>();
                    var item = this.node.ExtensibilityObject as ProjectItem;

                    names.AddRange(item.Properties
                        .Cast<Property>()
                        .Select(prop => prop.Name));

                    //if (item.ContainingProject != null)
                    //{
                    //    // MSBuild metadata enumeration is not working.
                    //    // Internally the non MPF projects don't store metadata
                    //    // in the MSBuild evaluated project itself, but on another 
                    //    // non-public representation.
                    //    var itemType = (string)item.Properties.Item("ItemType").Value;
                    //    var projectName = item.ContainingProject.FullName;
                    //    var projectDir = Path.GetDirectoryName(projectName);
                    //    var itemFullPath = new FileInfo(item.FileNames[1]).FullName;
                    //    var project = Microsoft.Build.Evaluation.ProjectCollection.GlobalProjectCollection.GetLoadedProjects(projectName).FirstOrDefault();
                    //    if (project != null)
                    //    {
                    //        names.AddRange(project.ItemsIgnoringCondition
                    //            .Where(i => i.ItemType == itemType)
                    //            .Select(i => new { Item = i, FullPath = new FileInfo(Path.Combine(projectDir, i.EvaluatedInclude)).FullName })
                    //            .Where(i => i.FullPath == itemFullPath)
                    //            .SelectMany(i => i.Item.Metadata.Select(m => m.Name)));
                    //    }
                    //}

                    return names;
                }
                catch
                {
                    return Enumerable.Empty<string>();
                }
            }
        }
    }
}
