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

namespace Clide.Solution
{
    using System;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Design;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using System.Collections.Generic;
    using System.Linq;
    using Clide.VisualStudio;

    [Export(typeof(IToolWindow))]
    [Export(typeof(ISolutionExplorer))]
    internal class SolutionExplorer : ISolutionExplorer
    {
        private VsToolWindow toolWindow;
        private Lazy<ITreeNodeFactory<IVsSolutionHierarchyNode>> nodeFactory;
        private IServiceProvider serviceProvider;

        [ImportingConstructor]
        public SolutionExplorer(
            [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
            [ImportMany(CompositionTarget.SolutionExplorer)] 
            IEnumerable<Lazy<ITreeNodeFactory<IVsSolutionHierarchyNode>, ITreeNodeFactoryMetadata>> nodeFactories)
        {
            Guard.NotNull(() => serviceProvider, serviceProvider);
            Guard.NotNull(() => nodeFactories, nodeFactories);

            this.serviceProvider = serviceProvider;
            this.toolWindow = serviceProvider.ToolWindow(StandardToolWindows.ProjectExplorer);
            this.nodeFactory = new Lazy<ITreeNodeFactory<IVsSolutionHierarchyNode>>(() =>
                new FallbackNodeFactory<IVsSolutionHierarchyNode>(
                    new AggregateNodeFactory<IVsSolutionHierarchyNode>(nodeFactories.Where(n => !n.Metadata.IsFallback).Select(f => f.Value)),
                    new AggregateNodeFactory<IVsSolutionHierarchyNode>(nodeFactories.Where(n => n.Metadata.IsFallback).Select(f => f.Value))));
        }

        public ISolutionNode Solution
        {
            get
            {
                return this.nodeFactory.Value.CreateNode(null,
                        new VsSolutionHierarchyNode(
                            this.serviceProvider.GetService<IVsSolution>() as IVsHierarchy, 
                            VSConstants.VSITEMID_ROOT))
                        .As<ISolutionNode>();
            }
        }

        public bool IsVisible
        {
            get { return this.toolWindow.IsVisible; }
        }

        public void Show()
        {
            this.toolWindow.Show();
        }

        public void Close()
        {
            this.toolWindow.Close();
        }

        public IEnumerable<ISolutionExplorerNode> SelectedNodes { get { return this.Solution.SelectedNodes; } }
    }
}
