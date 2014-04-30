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

namespace Clide.Sdk.Solution
{
    using Clide.Patterns.Adapter;
    using Clide.Solution;
    using Clide.Solution.Implementation;
    using Clide.VisualStudio;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using System.Linq;

    /// <summary>
    /// Default implementation of a managed project.
    /// </summary>
    public class ProjectNode : SolutionTreeNode, IProjectNode
	{
        private Lazy<GlobalProjectProperties> properties;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectNode"/> class.
        /// </summary>
        /// <param name="hierarchyNode">The underlying hierarchy represented by this node.</param>
        /// <param name="parentNode">The parent node accessor.</param>
        /// <param name="nodeFactory">The factory for child nodes.</param>
        /// <param name="adapter">The adapter service that implements the smart cast <see cref="ITreeNode.As{T}"/>.</param>
        public ProjectNode(
			IVsSolutionHierarchyNode hierarchyNode,
			Lazy<ITreeNode> parentNode,
			ITreeNodeFactory<IVsSolutionHierarchyNode> nodeFactory,
			IAdapterService adapter)
            : base(SolutionNodeKind.Project, hierarchyNode, parentNode, nodeFactory, adapter)
		{
            Guard.NotNull(() => parentNode, parentNode);

		    this.Project = new Lazy<EnvDTE.Project>(() => (EnvDTE.Project)hierarchyNode.VsHierarchy.Properties(hierarchyNode.ItemId).ExtenderObject);
            this.properties = new Lazy<GlobalProjectProperties>(() => new GlobalProjectProperties(this));
            this.Configuration = new ProjectConfiguration(this);
		}

        /// <summary>
        /// Gets the project active configuration information.
        /// </summary>
        public virtual IProjectConfiguration Configuration { get; private set; }

        /// <summary>
        /// Creates a folder inside the project.
        /// </summary>
        /// <param name="name">The name of the folder to create.</param>
        public virtual IFolderNode CreateFolder(string name)
		{
			Guard.NotNullOrEmpty(() => name, name);

			this.Project.Value.ProjectItems.AddFolder(name);

			var folder = this.HierarchyNode.Children
				.Single(child => child.VsHierarchy.Properties(child.ItemId).DisplayName == name);

			return this.CreateNode(folder) as IFolderNode;
		}

        /// <summary>
        /// Saves pending changes to the project file.
        /// </summary>
        public virtual void Save()
        {
            ErrorHandler.ThrowOnFailure(this
                .HierarchyNode
                .ServiceProvider
                .GetService<SVsSolution, IVsSolution>()
                .SaveSolutionElement(
                    (uint)__VSSLNSAVEOPTIONS.SLNSAVEOPT_ForceSave, 
                    this.HierarchyNode.VsHierarchy, 
                    0));
        }

        /// <summary>
        /// Gets the physical path of the project.
        /// </summary>
        public virtual string PhysicalPath
		{
			get
			{
				var dteProject = this.As<EnvDTE.Project>();
				if (dteProject == null)
					return null;
				else
					return dteProject.FullName;
			}
		}

        /// <summary>
        /// Gets the global properties of the project.
        /// </summary>
        /// <remarks>
        /// The default implementation for managed projects aggregates the 
        /// DTE properties and the MSBuild properties for the project. When 
        /// setting these properties, if an existing DTE property exists, 
        /// it's set, otherwise, an MSBuild property set is performed.
        /// </remarks>
        public virtual dynamic Properties
		{
			get { return this.properties.Value; }
		}

        /// <summary>
        /// Gets the configuration-specific properties for the project.
        /// </summary>
        /// <param name="configurationAndPlatform">Configuration names are the combination 
        /// of a project configuration and the platform, like "Debug|AnyCPU".</param>
        /// <remarks>
        /// To set properties for the current project configuration only, use 
        /// <c>project.PropertiesFor(project.Configuration.ActiveConfigurationName)</c>.
        /// </remarks>
        public virtual dynamic PropertiesFor(string configurationAndPlatform)
        {
            return new ConfigProjectProperties(this, configurationAndPlatform);
        }

        /// <summary>
        /// Gets the user-specific properties of the project.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public dynamic UserProperties
        {
            get { return new UserProjectProperties(this); }
        }

        /// <summary>
        /// Gets the configuration-specific user properties for the project.
        /// </summary>
        /// <param name="configurationName">Configuration names are the combination
        /// of a project configuration and the platform, like "Debug|AnyCPU".</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        /// <remarks>
        /// To set properties for the current project configuration only, use
        /// <c>project.UserPropertiesFor(project.Configuration.ActiveConfigurationName)</c>.
        /// </remarks>
        public dynamic UserPropertiesFor(string configurationName)
        {
            return new ConfigUserProjectProperties(this, configurationName);
        }

        /// <summary>
        /// Accepts the specified visitor for traversal.
        /// </summary>
        public override bool Accept(ISolutionVisitor visitor)
        {
            return SolutionVisitable.Accept(this, visitor);
        }

		/// <summary>
		/// Tries to smart-cast this node to the give type.
		/// </summary>
		/// <typeparam name="T">Type to smart-cast to.</typeparam>
		/// <returns>
		/// The casted value or null if it cannot be converted to that type.
		/// </returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public override T As<T>()
		{
			return this.Adapter.Adapt(this).As<T>();
		}

		/// <summary>
        /// Gets the DTE project represented by this node.
        /// </summary>
        internal Lazy<EnvDTE.Project> Project { get; private set; }
    }
}