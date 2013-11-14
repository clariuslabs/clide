#region BSD License
/* 
Copyright (c) 2012, Clarius Consulting
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

namespace Clide.Solution
{
    using Clide.Diagnostics;
    using Clide.Properties;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides usability extensions to the <see cref="ISolutionNode"/> interface.
    /// </summary>
    public static class ISolutionNodeExtensions
    {
        private static ITracer tracer = Tracer.Get(typeof(ISolutionNodeExtensions));

        /// <summary>
        /// Starts a build of the solution.
        /// </summary>
        public static Task<bool> Build(this ISolutionNode solution)
        {
            var sln = solution.As<EnvDTE.Solution>();
            if (sln == null)
                throw new ArgumentException(Strings.ISolutionNodeExtensions.BuildNotSupported);

            return System.Threading.Tasks.Task.Factory.StartNew<bool>(() =>
            {
                var mre = new ManualResetEventSlim();
                var events = sln.DTE.Events.BuildEvents;
                EnvDTE._dispBuildEvents_OnBuildDoneEventHandler done = (scope, action) => mre.Set();
                events.OnBuildDone += done;
                try
                {
                    // Let build run async.
                    sln.SolutionBuild.Build(false);

                    // Wait until it's done.
                    mre.Wait();

                    // LastBuildInfo == # of projects that failed to build.
                    return sln.SolutionBuild.LastBuildInfo == 0;
                }
                catch (Exception ex)
                {
                    tracer.Error(ex, Strings.ISolutionNodeExtensions.BuildException);
                    return false;
                }
                finally
                {
                    // Cleanup handler.
                    events.OnBuildDone -= done;
                }
            });
        }

        /// <summary>
        /// Finds all the project nodes in the solution.
        /// </summary>
        /// <param name="solution">The solution to traverse.</param>
        /// <returns>All project nodes that were found.</returns>
        public static IEnumerable<IProjectNode> FindProjects(this ISolutionNode solution)
        {
            var visitor = new ProjectsVisitor();

            solution.Accept(visitor);

            return visitor.Projects;
        }

        /// <summary>
        /// Finds all projects in the solution matching the given predicate.
        /// </summary>
        /// <param name="solution">The solution to traverse.</param>
        /// <param name="predicate">Predicate used to match projects.</param>
        /// <returns>All project nodes matching the given predicate that were found.</returns>
        public static IEnumerable<IProjectNode> FindProjects(this ISolutionNode solution, Func<IProjectNode, bool> predicate)
        {
            var visitor = new FilteringProjectsVisitor(predicate);

            solution.Accept(visitor);

            return visitor.Projects;
        }

        /// <summary>
        /// Finds the first project in the solution matching the given predicate.
        /// </summary>
        /// <param name="solution">The solution to traverse.</param>
        /// <param name="predicate">Predicate used to match projects.</param>
        /// <returns>The first project matching the given predicate, or <see langword="null"/>.</returns>
        public static IProjectNode FindProject(this ISolutionNode solution, Func<IProjectNode, bool> predicate)
        {
            var visitor = new FilteringProjectsVisitor(predicate, true);

            solution.Accept(visitor);

            return visitor.Projects.FirstOrDefault();
        }

        private class ProjectsVisitor : ISolutionVisitor
        {
            public ProjectsVisitor()
            {
                this.Projects = new List<IProjectNode>();
            }

            public List<IProjectNode> Projects { get; private set; }

            public bool VisitEnter(ISolutionNode solution)
            {
                return true;
            }

            public bool VisitLeave(ISolutionNode solution)
            {
                return true;
            }

            public bool VisitEnter(ISolutionItemNode solutionItem)
            {
                return false;
            }

            public bool VisitLeave(ISolutionItemNode solutionItem)
            {
                return true;
            }

            public bool VisitEnter(ISolutionFolderNode solutionFolder)
            {
                return true;
            }

            public bool VisitLeave(ISolutionFolderNode solutionFolder)
            {
                return true;
            }

            public bool VisitEnter(IProjectNode project)
            {
                Projects.Add(project);
                // Don't visit child nodes of a project since a 
                // project can't contain further projects.
                return false;
            }

            public bool VisitLeave(IProjectNode project)
            {
                return true;
            }

            public bool VisitEnter(IFolderNode folder)
            {
                throw new NotSupportedException();
            }

            public bool VisitLeave(IFolderNode folder)
            {
                throw new NotSupportedException();
            }

            public bool VisitEnter(IItemNode item)
            {
                throw new NotSupportedException();
            }

            public bool VisitLeave(IItemNode item)
            {
                throw new NotSupportedException();
            }

            public bool VisitEnter(IReferencesNode references)
            {
                throw new NotSupportedException();
            }

            public bool VisitLeave(IReferencesNode references)
            {
                throw new NotSupportedException();
            }

            public bool VisitEnter(IReferenceNode reference)
            {
                throw new NotSupportedException();
            }

            public bool VisitLeave(IReferenceNode reference)
            {
                throw new NotSupportedException();
            }
        }

        private class FilteringProjectsVisitor : ISolutionVisitor
        {
            private Func<IProjectNode, bool> predicate;
            private bool firstOnly;
            private bool done;

            public FilteringProjectsVisitor(Func<IProjectNode, bool> predicate, bool firstOnly = false)
            {
                this.predicate = predicate;
                this.firstOnly = firstOnly;
                this.Projects = new List<IProjectNode>();
            }

            public List<IProjectNode> Projects { get; private set; }

            /// <summary>
            /// Don't traverse project child elements.
            /// </summary>
            public bool VisitEnter(IProjectNode project)
            {
                if (!done && predicate(project))
                {
                    Projects.Add(project);
                    if (firstOnly)
                        done = true;
                }

                return false;
            }

            public bool VisitLeave(IProjectNode project)
            {
                return !done;
            }

            // Don't traverse child items of a solution item.
            public bool VisitEnter(ISolutionItemNode solutionItem)
            {
                return false;
            }

            public bool VisitLeave(ISolutionItemNode solutionItem)
            {
                return true;
            }

            public bool VisitEnter(ISolutionFolderNode solutionFolder)
            {
                return !done;
            }

            public bool VisitLeave(ISolutionFolderNode solutionFolder)
            {
                return !done;
            }

            public bool VisitEnter(ISolutionNode solution)
            {
                return true;
            }

            public bool VisitLeave(ISolutionNode solution)
            {
                return true;
            }

            public bool VisitEnter(IFolderNode folder)
            {
                throw new NotSupportedException();
            }

            public bool VisitLeave(IFolderNode folder)
            {
                throw new NotSupportedException();
            }

            public bool VisitEnter(IItemNode item)
            {
                throw new NotSupportedException();
            }

            public bool VisitLeave(IItemNode item)
            {
                throw new NotSupportedException();
            }

            public bool VisitEnter(IReferencesNode references)
            {
                throw new NotSupportedException();
            }

            public bool VisitLeave(IReferencesNode references)
            {
                throw new NotSupportedException();
            }

            public bool VisitEnter(IReferenceNode reference)
            {
                throw new NotSupportedException();
            }

            public bool VisitLeave(IReferenceNode reference)
            {
                throw new NotSupportedException();
            }
        }
    }
}