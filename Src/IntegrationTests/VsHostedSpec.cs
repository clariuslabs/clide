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

using Clide;
using Clide.Diagnostics;
using Clide.Events;
using EnvDTE;
using Microsoft.Practices.ServiceLocation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Microsoft.VSSDK.Tools.VsIdeTesting;
using Microsoft.VisualStudio.Shell;
using EnvDTE80;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

[TestClass]
public abstract class VsHostedSpec
{
	private ITracer tracer;
	private StringBuilder strings;
	private TraceListener listener;
	private List<string> cleanupFolders;

	public TestContext TestContext { get; set; }

	protected DTE2 Dte
	{
		get { return this.ServiceProvider.GetService<DTE, DTE2>(); }
	}

	protected IServiceProvider ServiceProvider
	{
		get { return GlobalServiceProvider.Instance.GetLoadedPackage(new Guid(IntegrationPackage.Constants.PackageGuid)); }
	}

	protected IServiceLocator ServiceLocator
	{
		get { return DevEnv.Get(this.ServiceProvider).ServiceLocator; }
	}

	[TestInitialize]
	public virtual void TestInitialize()
	{
		UIThreadInvoker.Initialize();

		// Causes devenv to initialize
		var devEnv = Clide.DevEnv.Get(new Guid(IntegrationPackage.Constants.PackageGuid));

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
			Dte.MainWindow.WindowState = EnvDTE.vsWindowState.vsWindowStateNormal;
		}

		var shellEvents = new ShellEvents(ServiceProvider);
		var initialized = shellEvents.IsInitialized;
		while (!initialized)
		{
			System.Threading.Thread.Sleep(10);
		}

		tracer.Info("Shell initialized successfully");
		if (VsIdeTestHostContext.ServiceProvider == null)
			VsIdeTestHostContext.ServiceProvider = new VsServiceProvider();

		cleanupFolders = new List<string>();
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

		if (Dte.Solution.IsOpen)
			CloseSolution();

		foreach (var folder in cleanupFolders)
		{
			try
			{
				Directory.Delete(folder, true);
			}
			catch { }
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "None")]
	protected string OpenSolution(string solutionFile)
	{
		if (!Path.IsPathRooted(solutionFile))
		{
			solutionFile = GetFullPath(TestContext.TestDeploymentDir, solutionFile);

			var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			cleanupFolders.Add(tempPath);

			CopyAll(Path.GetDirectoryName(solutionFile), tempPath);

			solutionFile = Path.Combine(tempPath, Path.GetFileName(solutionFile));
		}

		VsHostedSpec.DoActionWithWaitAndRetry(
			() => Dte.Solution.Open(solutionFile),
			2000,
			3,
			() => !Dte.Solution.IsOpen);

		return solutionFile;
	}

	protected void CloseSolution()
	{
		SpinWait.SpinUntil(() => Dte.Solution.SolutionBuild.BuildState == vsBuildState.vsBuildStateDone ||
			Dte.Solution.SolutionBuild.BuildState == vsBuildState.vsBuildStateNotStarted);

		VsHostedSpec.DoActionWithWaitAndRetry(
			() => Dte.Solution.Close(),
			2000,
			3,
			() => Dte.Solution.IsOpen);
	}

	/// <summary>
	/// Gets the full path relative to the test deployment directory.
	/// </summary>
	protected string GetFullPath(string relativePath)
	{
		return Path.Combine(TestContext.TestDeploymentDir, relativePath);
	}

	/// <summary>
	/// Gets the full path relative to the given base directory.
	/// </summary>
	protected string GetFullPath(string baseDir, string relativePath)
	{
		return Path.Combine(baseDir, relativePath);
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

	private static void CopyAll(string source, string target)
	{
		var directories = Directory.EnumerateDirectories(source, "*.*", SearchOption.AllDirectories);

		Parallel.ForEach(directories, dirPath =>
		{
			Directory.CreateDirectory(dirPath.Replace(source, target));
		});

		var files = Directory.EnumerateFiles(source, "*.*", SearchOption.AllDirectories);

		Parallel.ForEach(files, newPath =>
		{
			File.Copy(newPath, newPath.Replace(source, target));
		});
	}

	private class VsServiceProvider : IServiceProvider
	{
		public object GetService(Type serviceType)
		{
			return Package.GetGlobalService(serviceType);
		}
	}
}
