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

namespace Clide.Solution.Adapters
{
	using Clide.Patterns.Adapter;
	using Clide.Sdk.Solution;
	using Clide.Solution.Implementation;
	using Clide.VisualStudio;
	using EnvDTE;
	using Microsoft.VisualStudio;
	using Microsoft.VisualStudio.Shell.Interop;
	using System;

    [Adapter]
    internal class DteToVsAdapter :
        IAdapter<Solution, IVsSolution>,
        IAdapter<Project, IVsProject>,
        IAdapter<Project, VsHierarchyItem>,
        IAdapter<ProjectItem, VsHierarchyItem>
    {
        private IServiceProvider serviceProvider;

        public DteToVsAdapter(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IVsSolution Adapt(Solution from)
        {
			return this.serviceProvider.GetService<SVsSolution, IVsSolution>();
        }

        public IVsProject Adapt(Project from)
        {
            IVsHierarchy project;

			var uniqueName = "";
			try
			{
				// This might throw if the project isn't loaded yet.
				uniqueName = from.UniqueName;
			}
			catch (Exception)
			{
				// As a fallback, in C#/VB, the UniqueName == FullName.
				// It may still fail in the ext call though, but we do our best
				uniqueName = from.FullName;
			}

			if (!ErrorHandler.Succeeded(this.serviceProvider
                .GetService<SVsSolution, IVsSolution>()
                .GetProjectOfUniqueName(uniqueName, out project)))
                return null;

			return project as IVsProject;
        }

        VsHierarchyItem IAdapter<Project, VsHierarchyItem>.Adapt(Project from)
        {
            IVsHierarchy project;

			var uniqueName = "";
			try
			{
				// This might throw if the project isn't loaded yet.
				uniqueName = from.UniqueName;
			}
			catch (Exception)
			{
				// As a fallback, in C#/VB, the UniqueName == FullName.
				// It may still fail in the ext call though, but we do our best
				uniqueName = from.FullName;
			}

			if (!ErrorHandler.Succeeded(this.serviceProvider
                .GetService<SVsSolution, IVsSolution>()
                .GetProjectOfUniqueName(uniqueName, out project)))
                return null;

			return new VsHierarchyItem(project, VSConstants.VSITEMID_ROOT);
        }

        public VsHierarchyItem Adapt(ProjectItem from)
        {
            IVsHierarchy project;

			var uniqueName = "";
			try
			{
				// This might throw if the project isn't loaded yet.
				uniqueName = from.ContainingProject.UniqueName;
			}
			catch (Exception)
			{
				// As a fallback, in C#/VB, the UniqueName == FullName.
				// It may still fail in the ext call though, but we do our best
				uniqueName = from.ContainingProject.FullName;
			}

			if (!ErrorHandler.Succeeded(this.serviceProvider
                .GetService<SVsSolution, IVsSolution>()
                .GetProjectOfUniqueName(uniqueName, out project)))
                return null;

            var fileName = from.FileNames[0];
            var found = 0;
            uint itemId = 0;

            if (!ErrorHandler.Succeeded(((IVsProject)project).IsDocumentInProject(
                fileName, out found, new VSDOCUMENTPRIORITY[1], out itemId)) ||
                found == 0 || itemId == 0)
                return null;

            return new VsHierarchyItem(project, itemId);
        }
    }
}