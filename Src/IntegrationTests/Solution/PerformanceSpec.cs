namespace Clide.Solution
{
    using EnvDTE;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.VSSDK.Tools.VsIdeTesting;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;

    [TestClass]
    public class PerformanceSpec : VsHostedSpec
    {
        internal static readonly IAssertion Assert = new Assertion();

        [HostType("VS IDE")]
        [TestMethod]
        // Uncomment Ignore, and download EntLib to test performance.
        [Ignore]
        public void when_traversing_solution_to_find_projects_then_performance_is_acceptable()
        {
            OpenSolution(@"C:\Delete\Blocks\EnterpriseLibrary.sln");

            var solution = VsIdeTestHostContext.ServiceProvider.GetService<SVsSolution, IVsSolution>();

            var watch = Stopwatch.StartNew();
            var projects = GetProjects(solution, VSLangProj.PrjKind.prjKindCSharpProject);
            watch.Stop();

            var elapsed = watch.ElapsedTicks;

            var devenv = DevEnv.Get(VsIdeTestHostContext.ServiceProvider);
            // Warm-up factories.
            //devenv.SolutionExplorer().Solution.FindProjects().Select(node => node.As<Project>()).Where(prj => prj.Kind == VSLangProj.PrjKind.prjKindCSharpProject).ToArray();

            watch = Stopwatch.StartNew();
            projects = devenv.SolutionExplorer().Solution.FindProjects().Select(node => node.As<Project>()).ToArray();
            watch.Stop();

            Debug.WriteLine("IVS: {0}, Clide: {1}", elapsed, watch.ElapsedTicks);
        }

        // Traditional VS traversal via enumeration.
        public static EnvDTE.Project[] GetProjects(IVsSolution solution, string projectKind = null)
        {
            var projects = new List<Project>();

            if (solution == null)
            {
                return projects.ToArray();
            }

            IEnumHierarchies ppEnum;
            Guid tempGuid = Guid.Empty;
            solution.GetProjectEnum((uint)Microsoft.VisualStudio.Shell.Interop.__VSENUMPROJFLAGS.EPF_ALLPROJECTS, ref tempGuid, out ppEnum);
            if (ppEnum != null)
            {
                uint actualResult = 0;
                IVsHierarchy[] nodes = new IVsHierarchy[1];
                while (0 == ppEnum.Next(1, nodes, out actualResult))
                {
                    Object obj;
                    nodes[0].GetProperty((uint)Microsoft.VisualStudio.VSConstants.VSITEMID_ROOT, (int)Microsoft.VisualStudio.Shell.Interop.__VSHPROPID.VSHPROPID_ExtObject, out obj);
                    Project project = obj as Project;
                    if (project != null)
                    {
                        if (string.IsNullOrEmpty(projectKind))
                        {
                            projects.Add(project);
                        }
                        else if (projectKind.Equals(project.Kind, StringComparison.InvariantCultureIgnoreCase))
                        {
                            projects.Add(project);
                        }
                    }
                }
            }
            return projects.ToArray();
        }

        [HostType("VS IDE")]
        [TestMethod]
        // Comment Ignore, and download EntLib to test performance.
        [Ignore]
        public void when_retrieving_selected_project_then_performance_is_acceptable()
        {
            OpenSolution(@"C:\Delete\Blocks\EnterpriseLibrary.sln");

            var solution = this.ServiceProvider.GetService<SVsSolution, IVsSolution>();
            var devenv = DevEnv.Get(VsIdeTestHostContext.ServiceProvider);
            devenv.SolutionExplorer().Solution.FindProjects().First().Select();

            var iterations = 1000;

            var dteSelection = Measure(iterations, () => Console.WriteLine(GetSelectedProjectDte()));
            var ivsSelection = Measure(iterations, () => Console.WriteLine(GetSelectedProjectIVs()));
            var clideSelection = Measure(iterations, () => Console.WriteLine(GetSelectedProjectClide()));

            Debug.WriteLine("DTE Selection: {0}", dteSelection);
            Debug.WriteLine("IVs Selection: {0}", ivsSelection);
            Debug.WriteLine("Clide Selection: {0}", clideSelection);
        }

        [HostType("VS IDE")]
        [TestMethod]
        // Uncomment Ignore, and download EntLib to test performance.
        [Ignore]
        public void when_retrieving_active_project_then_performance_is_acceptable()
        {
            this.OpenSolution("SampleSolution\\SampleSolution.sln");

            var solution = VsIdeTestHostContext.ServiceProvider.GetService<SVsSolution, IVsSolution>();
            var devenv = DevEnv.Get(VsIdeTestHostContext.ServiceProvider);
            devenv.SolutionExplorer().Solution.FindProjects().First().Select();

            var iterations = 1000;

            //System.Diagnostics.Debugger.Launch();

            var dteSelection = Measure(iterations, () => Console.WriteLine(GetSelectedProjectDte()));
            var ivsSelection = Measure(iterations, () => Console.WriteLine(GetSelectedProjectIVs()));
            var clideSelection = Measure(iterations, () => Console.WriteLine(devenv.SolutionExplorer().Solution.ActiveProject.PhysicalPath));

            Debug.WriteLine("DTE Selection: {0}", dteSelection);
            Debug.WriteLine("IVs Selection: {0}", ivsSelection);
            Debug.WriteLine("Clide Selection: {0}", clideSelection);
        }

        private long Measure(int iterations, Action action)
        {
            var watch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                action();
            }
            watch.Stop();

            return watch.ElapsedTicks / iterations;
        }

        // Traditional DTE-based way of getting the current project.
        public static string GetSelectedProjectDte(DTE dte = null)
        {
            try
            {
                if (dte == null)
                    dte = Package.GetGlobalService(typeof(SDTE)) as DTE;
                if (dte == null)
                    return null;

                if (dte.Solution != null && dte.Solution.IsOpen)
                {
                    Array projects = dte.ActiveSolutionProjects as Array;
                    if (projects != null && projects.Length > 0)
                        return (projects.GetValue(0) as Project).FullName;
                }
                else
                {
                    return null;
                }
            }
            catch (COMException)
            {
                //tracer.Warn (ex, "Failed to retrieve current project.");
            }

            return null;
        }

        // Recommended IVs approach.
        public string GetSelectedProjectIVs()
        {
            IntPtr hierarchyPointer, selectionContainerPointer;
            Object selectedObject = null;
            IVsMultiItemSelect multiItemSelect;
            uint projectItemId;

            IVsMonitorSelection monitorSelection =
                    (IVsMonitorSelection)Package.GetGlobalService(
                    typeof(SVsShellMonitorSelection));

            monitorSelection.GetCurrentSelection(out hierarchyPointer,
                                                 out projectItemId,
                                                 out multiItemSelect,
                                                 out selectionContainerPointer);

            IVsHierarchy selectedHierarchy = null;
            try
            {
                selectedHierarchy = Marshal.GetTypedObjectForIUnknown(
                                                     hierarchyPointer,
                                                     typeof(IVsHierarchy)) as IVsHierarchy;
            }
            catch (Exception)
            {
                return null;
            }

            if (selectedHierarchy != null)
            {
                ErrorHandler.ThrowOnFailure(selectedHierarchy.GetProperty(
                                                  projectItemId,
                                                  (int)__VSHPROPID.VSHPROPID_ExtObject,
                                                  out selectedObject));
            }

            Project selectedProject = selectedObject as Project;

            return selectedProject.FullName;
        }

        public string GetSelectedProjectClide()
        {
            var devenv = DevEnv.Get(VsIdeTestHostContext.ServiceProvider);
            var project = devenv.SolutionExplorer().Solution.SelectedNodes.OfType<IProjectNode>().First();

            return project.PhysicalPath;
        }
    }
}
