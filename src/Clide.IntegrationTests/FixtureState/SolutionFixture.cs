using System;
using System.IO;
using System.Threading;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Xunit;

namespace Clide
{
	public class SolutionFixture : IDisposable, ISolutionFixture
	{
		// We cache this since it's sometimes changed by VS or a running test
		static string baseDirectory = Directory.GetCurrentDirectory();

		ISolutionNode solution;

		public SolutionFixture (string solutionFile)
		{
			if (!Path.IsPathRooted (solutionFile)) {
				var rootedFile = Path.Combine (Path.GetDirectoryName (GetType ().Assembly.ManifestModule.FullyQualifiedName), solutionFile);
				if (!File.Exists (rootedFile)) {
					rootedFile = Path.Combine (baseDirectory, solutionFile);
					var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
					while (!File.Exists (rootedFile) && currentDir != null) {
						rootedFile = Path.Combine (currentDir.FullName, solutionFile);
						currentDir = currentDir.Parent;
					}
				}

				solutionFile = rootedFile;
			}

			if (!File.Exists (solutionFile))
				throw new FileNotFoundException ("Could not find solution file " + solutionFile, solutionFile);

			try {
				var dte = GlobalServices.GetService<DTE>();
				if (!dte.Solution.IsOpen || !dte.Solution.FullName.Equals (solutionFile, StringComparison.OrdinalIgnoreCase)) {
					// Ensure no .suo is loaded, since that would dirty the state across runs.
					var suoFile = Path.ChangeExtension(solutionFile, ".suo");
					if (File.Exists (suoFile))
						Try (() => File.Delete (suoFile));

					var sdfFile = Path.ChangeExtension(solutionFile, ".sdf");
					if (File.Exists (sdfFile))
						Try (() => File.Delete (sdfFile));

					var vsDir = Path.Combine(Path.GetDirectoryName(solutionFile), ".vs");
					if (Directory.Exists (vsDir))
						Try (() => Directory.Delete (vsDir, true));

					dte.Solution.Open (solutionFile);
					GlobalServices.GetService<SVsSolution, IVsSolution4> ()
						.EnsureSolutionIsLoaded ((uint)(__VSBSLFLAGS.VSBSLFLAGS_LoadAllPendingProjects | __VSBSLFLAGS.VSBSLFLAGS_LoadBuildDependencies));
				}

				solution = GlobalServices.GetService<SComponentModel, IComponentModel> ().GetService<ISolutionExplorer> ().Solution;
			} catch (Exception ex) {
				throw new ArgumentException ("Failed to open and access solution: " + solutionFile, ex);
			}
		}

		void Try (Action action)
		{
			try {
				action ();
			} catch { }
		}

		public ISolutionNode Solution => solution;

		public void Dispose ()
		{
			Solution.Close ();
		}

		class SolutionLoadEvents : IVsSolutionLoadEvents
		{
			Action onLoadComplete;

			public SolutionLoadEvents (Action onLoadComplete)
			{
				this.onLoadComplete = onLoadComplete;
			}

			public int OnAfterBackgroundSolutionLoadComplete ()
			{
				onLoadComplete ();
				return VSConstants.S_OK;
			}

			public int OnAfterLoadProjectBatch (bool fIsBackgroundIdleBatch) => VSConstants.S_OK;

			public int OnBeforeBackgroundSolutionLoadBegins () => VSConstants.S_OK;

			public int OnBeforeLoadProjectBatch (bool fIsBackgroundIdleBatch) => VSConstants.S_OK;

			public int OnBeforeOpenSolution (string pszSolutionFilename) => VSConstants.S_OK;

			public int OnQueryBackgroundLoadProjectBatch (out bool pfShouldDelayLoadToNextIdle)
			{
				pfShouldDelayLoadToNextIdle = false;
				return VSConstants.S_OK;
			}
		}
	}
}
