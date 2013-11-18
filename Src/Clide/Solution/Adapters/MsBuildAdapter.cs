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
    using Microsoft.Build.Evaluation;
    using System.IO;
    using System.Linq;

    [Adapter]
    internal class MsBuildAdapter :
        IAdapter<ProjectNode, Project>,
        IAdapter<ItemNode, ProjectItem>
    {
        public Project Adapt(ProjectNode from)
        {
            return from == null || from.Project.Value == null ? null :
                ProjectCollection.GlobalProjectCollection
                    .GetLoadedProjects(from.Project.Value.FullName)
                    .FirstOrDefault();
        }

        public ProjectItem Adapt(ItemNode from)
        {
            if (from == null || from.Item.Value == null || from.Item.Value.ContainingProject == null)
                return null;

            var item = from.Item.Value;
            var itemType = (string)item.Properties.Item("ItemType").Value;
            var itemFullPath = new FileInfo(item.FileNames[1]).FullName;

            var projectName = item.ContainingProject.FullName;
            var projectDir = Path.GetDirectoryName(projectName);
            var project = ProjectCollection.GlobalProjectCollection.GetLoadedProjects(projectName).FirstOrDefault();

            if (project != null)
            {
                return project.ItemsIgnoringCondition
                    .Where(i => i.ItemType == itemType)
                    .Select(i => new { Item = i, FullPath = new FileInfo(Path.Combine(projectDir, i.EvaluatedInclude)).FullName })
                    .Where(i => i.FullPath == itemFullPath)
                    .Select(i => i.Item)
                    .FirstOrDefault();
            }

            return null;
        }
    }
}
