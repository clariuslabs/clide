using System;
using System.IO;
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

        string tempDir;
        string solutionFile;
        bool closeSolutionOnDispose;

        public SolutionFixture(string solutionFile, bool useCopy = false)
        {
            if (!Path.IsPathRooted(solutionFile))
            {
                var rootedFile = Path.Combine(Path.GetDirectoryName(GetType().Assembly.ManifestModule.FullyQualifiedName), solutionFile);
                if (!File.Exists(rootedFile))
                {
                    rootedFile = Path.Combine(baseDirectory, solutionFile);
                    var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
                    while (!File.Exists(rootedFile) && currentDir != null)
                    {
                        rootedFile = Path.Combine(currentDir.FullName, solutionFile);
                        currentDir = currentDir.Parent;
                    }
                }

                solutionFile = rootedFile;
            }

            this.solutionFile = solutionFile;

            if (!File.Exists(solutionFile))
                throw new FileNotFoundException("Could not find solution file " + solutionFile, solutionFile);

            if (useCopy)
            {
                tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                if (!Directory.Exists(tempDir))
                    Directory.CreateDirectory(tempDir);

                DirectoryCopy(Path.GetDirectoryName(solutionFile), tempDir, true);
                solutionFile = Path.Combine(tempDir, Path.GetFileName(solutionFile));
            }

            EnsureSolutionIsLoaded();
        }

        void Try(Action action)
        {
            try
            {
                action();
            }
            catch { }
        }

        public ISolutionNode Solution =>
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                EnsureSolutionIsLoaded();

                return await GlobalServices.GetService<SComponentModel, IComponentModel>().GetService<ISolutionExplorer>().Solution;
            });

        public void EnsureSolutionIsLoaded()
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                var dte = GlobalServices.GetService<DTE>();
                if (!dte.Solution.IsOpen || !dte.Solution.FullName.Equals(this.solutionFile, StringComparison.OrdinalIgnoreCase))
                {
                    // Ensure no .suo is loaded, since that would dirty the state across runs.
                    var suoFile = Path.ChangeExtension(this.solutionFile, ".suo");
                    if (File.Exists(suoFile))
                        Try(() => File.Delete(suoFile));

                    var sdfFile = Path.ChangeExtension(this.solutionFile, ".sdf");
                    if (File.Exists(sdfFile))
                        Try(() => File.Delete(sdfFile));

                    var vsDir = Path.Combine(Path.GetDirectoryName(this.solutionFile), ".vs");
                    if (Directory.Exists(vsDir))
                        Try(() => Directory.Delete(vsDir, true));

                    dte.Solution.Open(this.solutionFile);
                    ErrorHandler.ThrowOnFailure(GlobalServices.GetService<SVsSolution, IVsSolution4>()
                        .EnsureSolutionIsLoaded((uint)(__VSBSLFLAGS.VSBSLFLAGS_LoadAllPendingProjects | __VSBSLFLAGS.VSBSLFLAGS_LoadBuildDependencies)));

                    closeSolutionOnDispose = true;
                }
            });
        }

        public void Dispose()
        {
            // Only close the solution if the solution was opened at all.
            if (closeSolutionOnDispose)
            {
                var dte = GlobalServices.GetService<DTE>();
                if (dte != null)
                    dte.Solution.Close(false);
            }
        }

        static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            var dir = new DirectoryInfo(sourceDirName);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
                Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            foreach (var file in dir.GetFiles())
            {
                var tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (var subdir in dir.GetDirectories())
                {
                    var tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }
    }
}
