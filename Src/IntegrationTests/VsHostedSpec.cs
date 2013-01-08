using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;
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
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;
using System.Collections;
using System.ComponentModel.Composition.Primitives;

[TestClass]
public abstract class VsHostedSpec
{
    static ConsoleTraceListener listener = new ConsoleTraceListener();

    static VsHostedSpec()
    {
        //Tracer.Initialize(new TracerManager());
        Tracer.Manager.AddListener(TracerManager.DefaultSourceName, listener);
    }

    protected VsHostedSpec()
    {
        this.integrationPackage = new Lazy<ShellPackage>(() => LoadPackage());
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
        get { return (CompositionContainer)DevEnv.Get(this.ServiceProvider).CompositionService; }
    }

    private Lazy<ShellPackage> integrationPackage;

    private ShellPackage LoadPackage()
    {
        //Debug.Fail("Attach");
        return this.ServiceProvider.GetLoadedPackage<ShellPackage>();
    }

    protected ShellPackage ShellPackage { get { return this.integrationPackage.Value; } }

    [TestInitialize]
    public virtual void TestInitialize()
    {
        Console.WriteLine("Running test from: " + this.TestContext.TestDeploymentDir);

        if (Dte != null)
        {
            Dte.SuppressUI = false;
            Dte.MainWindow.Visible = true;
            Dte.MainWindow.WindowState = EnvDTE.vsWindowState.vsWindowStateMaximize;
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
        var contextData = (Hashtable)ExecutionContext
            .Capture()
            .AsDynamicReflection()
            .LogicalCallContext
            .Datastore;

        foreach (var slot in contextData.Keys.OfType<string>())
        {
            CallContext.FreeNamedDataSlot(slot);
        }

        listener.Flush();
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
