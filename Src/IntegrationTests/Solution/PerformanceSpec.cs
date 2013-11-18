namespace Clide.Solution
{
    using EnvDTE;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.VSSDK.Tools.VsIdeTesting;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

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
    }
}
