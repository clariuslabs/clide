#region BSD License
/* 
Copyright (c) 2012, Clarius Consulting
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

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
using Clide.Diagnostics;
using Microsoft.VisualStudio.Shell;
using Clide.Events;
using Clide.VisualStudio;
using Microsoft.Practices.ServiceLocation;

[TestClass]
public abstract class VsHostedSpec
{
    private Lazy<IServiceProvider> package;
    private ITracer tracer;
    private StringBuilder strings; 
    private TraceListener listener;

    public TestContext TestContext { get; set; }

    protected EnvDTE.DTE Dte
    {
        get { return this.ServiceProvider.GetService<DTE>(); }
    }

    protected IServiceProvider ServiceProvider
    {
        get { return GlobalServiceProvider.Instance; }
    }

    protected IServiceLocator ServiceLocator
    {
        get { return DevEnv.Get(this.ServiceProvider).ServiceLocator; }
    }

    [TestInitialize]
    public virtual void TestInitialize()
    {
        // Causes devenv to initialize
        var factory = DevEnv.DevEnvFactory;

        this.tracer = Tracer.Get(this.GetType());
        this.strings = new StringBuilder();
        this.listener = new TextWriterTraceListener(new StringWriter(this.strings));

        // Just in case, re-set the tracers.
        Tracer.Manager.SetTracingLevel(TracerManager.DefaultSourceName, SourceLevels.All);
        Tracer.Manager.AddListener(TracerManager.DefaultSourceName, this.listener);

        tracer.Info("Running test from: " + this.TestContext.TestDeploymentDir);

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

        tracer.Info("Shell initialized successfully");
    }

    [TestCleanup]
    public virtual void TestCleanup()
    {
        //tracer.Info("Cleaning ambient context data");
        //var contextData = (Hashtable)ExecutionContext
        //    .Capture()
        //    .AsDynamicReflection()
        //    .LogicalCallContext
        //    .Datastore;

        //foreach (var slot in contextData.Keys.OfType<string>())
        //{
        //    CallContext.FreeNamedDataSlot(slot);
        //}

        listener.Flush();
        Debug.WriteLine(this.strings.ToString());
        Console.WriteLine(this.strings.ToString());
        Trace.WriteLine(this.strings.ToString());
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
