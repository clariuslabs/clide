using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using Microsoft.VisualStudio.ComponentModelHost;
using System.ComponentModel.Composition.Hosting;
using Clide;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using IntegrationPackage;
using EnvDTE;

[TestClass]
public abstract class VsHostedSpec
{
    protected VsHostedSpec()
    {
        this.integrationPackage = new Lazy<ShellPackage>(() => GetPackage());
    }

    public TestContext TestContext { get; set; }

    protected EnvDTE.DTE Dte
    {
        get { return ServiceProvider.GetService<DTE>(); }
    }

    protected IServiceProvider ServiceProvider
    {
        get { return Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider; }
    }

    protected CompositionContainer Container
    {
        get { return (CompositionContainer)this.IntegrationPackage.DevEnv.CompositionService; }
    }

    private Lazy<ShellPackage> integrationPackage;

    private ShellPackage GetPackage()
    {
        return (ShellPackage)this.ServiceProvider.GetExportedValue<IShellPackage>();
    }

    protected ShellPackage IntegrationPackage { get { return this.integrationPackage.Value; } }

    [TestInitialize]
    public virtual void TestInitialize()
    {
        if (Dte != null)
        {
            Dte.SuppressUI = false;
            Dte.MainWindow.Visible = true;
            Dte.MainWindow.WindowState = EnvDTE.vsWindowState.vsWindowStateMaximize;
            GetPackage();
        }

        var shellEvents = new ShellEvents(ServiceProvider);
        var initialized = shellEvents.IsInitialized;
        while (!initialized)
        {
            System.Threading.Thread.Sleep(10);
        }
    }

    [TestCleanup]
    public virtual void TestCleanup()
    {
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "None")]
    protected void OpenSolution(string solutionFile)
    {
        if (!Path.IsPathRooted(solutionFile))
            solutionFile = GetFullPath(solutionFile);

        VsHostedSpec.DoActionWithWaitAndRetry(
            () => Dte.Solution.Open(solutionFile),
            2000,
            3,
            () => !Dte.Solution.IsOpen);
    }

    protected void CloseSolution()
    {
        VsHostedSpec.DoActionWithWaitAndRetry(
            () => Dte.Solution.Close(),
            2000,
            3,
            () => Dte.Solution.IsOpen);
    }

    protected string GetFullPath(string relativePath)
    {
        return Path.Combine(TestContext.TestDeploymentDir, relativePath);
    }

    protected static void DoActionWithWait(Action action, int millisecondsToWait)
    {
        DoActionWithWaitAndRetry(action, millisecondsToWait, 1, () => true);
    }

    protected static void DoActionWithWaitAndRetry(Action action, int millisecondsToWait, int numberOfRetries, Func<bool> retryCondition)
    {
        int retry = 0;

        do
        {
            action();
            if (retryCondition())
            {
                System.Threading.Thread.Sleep(millisecondsToWait);
                Application.DoEvents();
                retry++;
            }
        }
        while (retryCondition() && retry < numberOfRetries);
    }
}
