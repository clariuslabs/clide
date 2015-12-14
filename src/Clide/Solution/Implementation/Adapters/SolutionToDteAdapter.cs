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
    using EnvDTE;
    using EnvDTE80;
    using VSLangProj;

    [Adapter]
    internal class SolutionToDteAdapter :
        IAdapter<SolutionNode, Solution>,
        IAdapter<SolutionFolderNode, SolutionFolder>,
        IAdapter<ProjectNode, Project>,
        IAdapter<FolderNode, ProjectItem>,
        IAdapter<ItemNode, ProjectItem>
    {
        public Solution Adapt(SolutionNode from)
        {
            return from.Solution.Value;
        }

        public SolutionFolder Adapt(SolutionFolderNode from)
        {
            return from.SolutionFolder.Value;
        }

        public Project Adapt(ProjectNode from)
        {
            return from.Project.Value;
        }

        public ProjectItem Adapt(FolderNode from)
        {
            return from.Folder.Value;
        }

        public ProjectItem Adapt(ItemNode from)
        {
            return from.Item.Value;
        }
    }
}
