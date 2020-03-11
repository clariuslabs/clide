using System;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Xunit;

namespace Clide
{
    [Trait("Feature", "Service Locator")]
    public class ServiceLocatorSpec
    {
        [VsixFact]
        public void when_requesting_locator_for_package_guid_then_loads_package()
        {
            var shell = GlobalServices.GetService<SVsShell, IVsShell>();
            var guid = Constants.SdkPackageGuid;
            IVsPackage package = null;

            Assert.Equal(VSConstants.E_FAIL, shell.IsPackageLoaded(guid, out package));
            Assert.Null(package);

            var locator = ServiceLocator.Get(Constants.SdkPackageGuid);

            Assert.Equal(VSConstants.S_OK, shell.IsPackageLoaded(guid, out package));
            Assert.NotNull(package);
        }
    }

    [Trait("Feature", "Service Locator")]
    [Collection("SingleProject")]
    public class ServiceLocatorExtensionsSpec
    {
        [VsixFact]
        public void when_requesting_locator_from_service_provider_then_succeeds()
        {
            IServiceProvider services = GlobalServices.Instance;
            var locator = services.GetServiceLocator();

            Assert.NotNull(locator);
        }

        [VsixFact]
        public void when_requesting_locator_from_dte_then_succeeds()
        {
            var services = GlobalServices.GetService<DTE>();
            var locator = services.GetServiceLocator();

            Assert.NotNull(locator);
        }

        [VsixFact]
        public void when_requesting_locator_from_dte_project_then_succeeds()
        {
            var dte = GlobalServices.GetService<DTE>();
            var sln = (Solution2)dte.Solution;

            var proj = dte.Solution.Projects.OfType<Project>().First();

            var locator = proj.GetServiceLocator();

            Assert.NotNull(locator);
        }

        [VsixFact]
        public void when_requesting_locator_from_hierarchy_then_succeeds()
        {
            var dte = GlobalServices.GetService<DTE>();
            var sln = (Solution2)dte.Solution;

            var proj = dte.Solution.Projects.OfType<Project>().First();
            var vssln = GlobalServices.GetService<SVsSolution, IVsSolution>();
            var hier = default(IVsHierarchy);

            ErrorHandler.ThrowOnFailure(vssln.GetProjectOfUniqueName(proj.UniqueName, out hier));

            var locator = hier.GetServiceLocator();

            Assert.NotNull(locator);
        }

        [VsixFact]
        public void when_requesting_locator_from_vsproject_then_succeeds()
        {
            var dte = GlobalServices.GetService<DTE>();
            var sln = (Solution2)dte.Solution;

            var proj = dte.Solution.Projects.OfType<Project>().First();
            var vssln = GlobalServices.GetService<SVsSolution, IVsSolution>();
            IVsHierarchy hier;
            ErrorHandler.ThrowOnFailure(vssln.GetProjectOfUniqueName(proj.UniqueName, out hier));

            var vsproj = hier as IVsProject;
            Assert.NotNull(hier);
            Assert.NotNull(vsproj);

            var locator = vsproj.GetServiceLocator();

            Assert.NotNull(locator);
        }

        [VsixFact]
        public void when_getting_vs_service_from_locator_from_service_provider_then_succeeds()
        {
            IServiceProvider services = GlobalServices.Instance;
            var locator = services.GetServiceLocator();

            Assert.NotNull(locator.GetService(typeof(SVsShell)));
        }

        [VsixFact]
        public void when_getting_vs_service_from_locator_from_dte_then_succeeds()
        {
            var services = GlobalServices.GetService<DTE>();
            var locator = services.GetServiceLocator();

            Assert.NotNull(locator.GetService(typeof(SVsShell)));
        }

        [VsixFact]
        public void when_getting_vs_service_from_locator_from_dte_project_then_succeeds()
        {
            var dte = GlobalServices.GetService<DTE>();
            var sln = (Solution2)dte.Solution;

            var proj = dte.Solution.Projects.OfType<Project>().First();

            var locator = proj.GetServiceLocator();

            Assert.NotNull(locator.GetService(typeof(SVsShell)));
        }

        [VsixFact]
        public void when_getting_vs_service_from_locator_from_hierarchy_then_succeeds()
        {
            var dte = GlobalServices.GetService<DTE>();
            var sln = (Solution2)dte.Solution;

            var proj = dte.Solution.Projects.OfType<Project>().First();
            var vssln = GlobalServices.GetService<SVsSolution, IVsSolution>();
            IVsHierarchy hier;
            ErrorHandler.ThrowOnFailure(vssln.GetProjectOfUniqueName(proj.UniqueName, out hier));

            var locator = hier.GetServiceLocator();

            Assert.NotNull(locator.GetService(typeof(SVsShell)));
        }

        [VsixFact]
        public void when_getting_vs_service_from_locator_from_vsproject_then_succeeds()
        {
            var dte = GlobalServices.GetService<DTE>();
            var sln = (Solution2)dte.Solution;

            var proj = dte.Solution.Projects.OfType<Project>().First();
            var vssln = GlobalServices.GetService<SVsSolution, IVsSolution>();
            IVsHierarchy hier;
            ErrorHandler.ThrowOnFailure(vssln.GetProjectOfUniqueName(proj.UniqueName, out hier));

            var vsproj = hier as IVsProject;
            Assert.NotNull(hier);
            Assert.NotNull(vsproj);

            var locator = vsproj.GetServiceLocator();

            Assert.NotNull(locator.GetService(typeof(SVsShell)));
        }

        [VsixFact]
        public void when_getting_vs_export_from_locator_from_service_provider_then_succeeds()
        {
            IServiceProvider services = GlobalServices.Instance;
            var locator = services.GetServiceLocator();

            Assert.NotNull(locator.GetExport<SVsServiceProvider>());
        }

        [VsixFact]
        public void when_getting_vs_export_from_locator_from_dte_then_succeeds()
        {
            var services = GlobalServices.GetService<DTE>();
            var locator = services.GetServiceLocator();

            Assert.NotNull(locator.GetExport<SVsServiceProvider>());
        }

        [VsixFact]
        public void when_getting_vs_export_from_locator_from_dte_project_then_succeeds()
        {
            var dte = GlobalServices.GetService<DTE>();
            var sln = (Solution2)dte.Solution;

            var proj = dte.Solution.Projects.OfType<Project>().First();

            var locator = proj.GetServiceLocator();

            Assert.NotNull(locator.GetExport<SVsServiceProvider>());
        }

        [VsixFact]
        public void when_getting_vs_export_from_locator_from_hierarchy_then_succeeds()
        {
            var dte = GlobalServices.GetService<DTE>();
            var sln = (Solution2)dte.Solution;

            var proj = dte.Solution.Projects.OfType<Project>().First();
            var vssln = GlobalServices.GetService<SVsSolution, IVsSolution>();
            IVsHierarchy hierarchy;
            ErrorHandler.ThrowOnFailure(vssln.GetProjectOfUniqueName(proj.UniqueName, out hierarchy));
            Assert.NotNull(hierarchy);

            var locator = hierarchy.GetServiceLocator();

            Assert.NotNull(locator.GetExport<SVsServiceProvider>());
        }

        [VsixFact]
        public void when_getting_vs_export_from_locator_from_vsproject_then_succeeds()
        {
            var dte = GlobalServices.GetService<DTE>();
            var sln = (Solution2)dte.Solution;

            var proj = dte.Solution.Projects.OfType<Project>().First();
            var vssln = GlobalServices.GetService<SVsSolution, IVsSolution>();
            IVsHierarchy hierarchy;
            ErrorHandler.ThrowOnFailure(vssln.GetProjectOfUniqueName(proj.UniqueName, out hierarchy));

            var vsproj = hierarchy as IVsProject;
            Assert.NotNull(vsproj);

            var locator = vsproj.GetServiceLocator();

            Assert.NotNull(locator.GetExport<SVsServiceProvider>());
        }

        [VsixFact]
        public void when_getting_vs_exports_from_locator_from_service_provider_then_succeeds()
        {
            IServiceProvider services = GlobalServices.Instance;
            var locator = services.GetServiceLocator();

            Assert.True(locator.GetExports<IWpfTextViewCreationListener>().Any());
        }

        [VsixFact]
        public void when_getting_vs_exports_from_locator_from_dte_then_succeeds()
        {
            var services = GlobalServices.GetService<DTE>();
            var locator = services.GetServiceLocator();

            Assert.True(locator.GetExports<IWpfTextViewCreationListener>().Any());
        }

        [VsixFact]
        public void when_getting_vs_exports_from_locator_from_dte_project_then_succeeds()
        {
            var dte = GlobalServices.GetService<DTE>();
            var sln = (Solution2)dte.Solution;

            var proj = dte.Solution.Projects.OfType<Project>().First();

            var locator = proj.GetServiceLocator();

            Assert.True(locator.GetExports<IWpfTextViewCreationListener>().Any());
        }

        [VsixFact]
        public void when_getting_vs_exports_from_locator_from_hierarchy_then_succeeds()
        {
            var dte = GlobalServices.GetService<DTE>();
            var sln = (Solution2)dte.Solution;

            var proj = dte.Solution.Projects.OfType<Project>().First();
            var vssln = GlobalServices.GetService<SVsSolution, IVsSolution>();
            IVsHierarchy hierarchy;
            ErrorHandler.ThrowOnFailure(vssln.GetProjectOfUniqueName(proj.UniqueName, out hierarchy));
            Assert.NotNull(hierarchy);

            var locator = hierarchy.GetServiceLocator();

            Assert.True(locator.GetExports<IWpfTextViewCreationListener>().Any());
        }

        [VsixFact]
        public void when_getting_vs_exports_from_locator_from_vsproject_then_succeeds()
        {
            var dte = GlobalServices.GetService<DTE>();
            var sln = (Solution2)dte.Solution;

            var proj = dte.Solution.Projects.OfType<Project>().First();
            var vssln = GlobalServices.GetService<SVsSolution, IVsSolution>();
            IVsHierarchy hierarchy;
            ErrorHandler.ThrowOnFailure(vssln.GetProjectOfUniqueName(proj.UniqueName, out hierarchy));
            Assert.NotNull(hierarchy);

            var vsproj = hierarchy as IVsProject;
            Assert.NotNull(vsproj);

            var locator = vsproj.GetServiceLocator();

            Assert.True(locator.GetExports<IWpfTextViewCreationListener>().Any());
        }
    }
}
