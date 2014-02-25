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
    using Clide.Properties;
    using Clide.Sdk.Solution;
    using Clide.Solution;
    using Clide.Solution.Implementation;
    using Clide.VisualStudio;
    using EnvDTE;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Default base implementation of <see cref="ISolutionExplorerNode"/>.
    /// </summary>
    [DebuggerDisplay("{debuggerDisplay,nq}")]
    public abstract class SolutionTreeNode : ISolutionExplorerNode
    {
        private string debuggerDisplay;
        private IVsSolutionHierarchyNode hierarchyNode;
        private ITreeNodeFactory<IVsSolutionHierarchyNode> factory;
        private IAdapterService adapter;
        private Lazy<IVsUIHierarchyWindow> window;
        private Lazy<ITreeNode> parent;
        private Lazy<bool> isHidden;
        private Lazy<ISolutionNode> solutionNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionTreeNode"/> class.
        /// </summary>
        /// <param name="nodeKind">Kind of the node.</param>
        /// <param name="hierarchyNode">The underlying hierarchy represented by this node.</param>
        /// <param name="parentNode">The parent node accessor.</param>
        /// <param name="nodeFactory">The factory for child nodes.</param>
        /// <param name="adapter">The adapter service that implements the smart cast <see cref="ITreeNode.As{T}"/>.</param>
        protected SolutionTreeNode(
            SolutionNodeKind nodeKind,
            IVsSolutionHierarchyNode hierarchyNode,
            Lazy<ITreeNode> parentNode,
            ITreeNodeFactory<IVsSolutionHierarchyNode> nodeFactory,
            IAdapterService adapter)
        {
            Guard.NotNull(() => hierarchyNode, hierarchyNode);
            Guard.NotNull(() => nodeFactory, nodeFactory);
            Guard.NotNull(() => adapter, adapter);

            this.hierarchyNode = hierarchyNode;
            this.factory = nodeFactory;
            this.adapter = adapter;
            this.window = new Lazy<IVsUIHierarchyWindow>(() => GetWindow(this.hierarchyNode.ServiceProvider));
            this.parent = parentNode ?? new Lazy<ITreeNode>(() => null);
            this.DisplayName = this.hierarchyNode.VsHierarchy.Properties(hierarchyNode.ItemId).DisplayName;
            this.Kind = nodeKind;

            Func<bool> getHiddenProperty = () => GetProperty<bool?>(
                this.hierarchyNode.VsHierarchy,
                __VSHPROPID.VSHPROPID_IsHiddenItem,
                this.hierarchyNode.ItemId).GetValueOrDefault();

            this.isHidden = parentNode != null ?
                new Lazy<bool>(() => getHiddenProperty() || parentNode.Value.IsHidden) :
                new Lazy<bool>(() => getHiddenProperty());

            if (System.Diagnostics.Debugger.IsAttached)
                this.debuggerDisplay = BuildDebuggerDisplay();

            this.solutionNode = new Lazy<ISolutionNode>(() => 
            {
                var solutionHierarchy = new VsSolutionHierarchyNode(
                    (IVsHierarchy)this.hierarchyNode.ServiceProvider.GetService<SVsSolution, IVsSolution>(),
                    VSConstants.VSITEMID_ROOT);
                
                return (ISolutionNode)this.factory.CreateNode(null, solutionHierarchy);
            });
        }

        /// <summary>
        /// Gets the underlying hierarchy node represented by this node.
        /// </summary>
        protected internal IVsSolutionHierarchyNode HierarchyNode { get { return this.hierarchyNode; } }

		/// <summary>
		/// Gets the adapter service used to construct this node.
		/// </summary>
		protected IAdapterService Adapter { get { return this.adapter; } }

        /// <summary>
        /// Gets the parent of this node.
        /// </summary>
        public virtual ITreeNode Parent { get { return this.parent.Value; } }

        /// <summary>
        /// Gets the owning solution.
        /// </summary>
        public virtual ISolutionNode OwningSolution { get { return this.solutionNode.Value; } }

        /// <summary>
        /// Gets the node display name.
        /// </summary>
        public virtual string DisplayName { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this node is hidden.
        /// </summary>
        public virtual bool IsHidden
        {
            get { return this.isHidden.Value; }
        }

        /// <summary>
        /// Gets a value indicating whether this node is visible.
        /// </summary>
        public virtual bool IsVisible
        {
            get
            {
                // If node is not null
                return !this.IsHidden && 
                    // And either parent is null or it's visible and expanded
                    (this.Parent == null || (this.Parent.IsVisible && this.Parent.IsExpanded));
            }
        }

        /// <summary>
        /// Gets a value indicating whether this node is selected.
        /// </summary>
        public virtual bool IsSelected
        {
            get
            {
                uint state;
                ErrorHandler.ThrowOnFailure(this.window.Value.GetItemState(
                    this.hierarchyNode.VsHierarchy as IVsUIHierarchy, this.hierarchyNode.ItemId, (uint)__VSHIERARCHYITEMSTATE.HIS_Selected, out state));

                return state == (uint)__VSHIERARCHYITEMSTATE.HIS_Selected;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this node is expanded.
        /// </summary>
        public virtual bool IsExpanded
        {
            get
            {
                // The solution node itself is always expanded.
                if (this.Parent == null)
                    return true;

                uint state;
                ErrorHandler.ThrowOnFailure(this.window.Value.GetItemState(
                    this.hierarchyNode.VsHierarchy as IVsUIHierarchy, this.hierarchyNode.ItemId, (uint)__VSHIERARCHYITEMSTATE.HIS_Expanded, out state));

                return state == (uint)__VSHIERARCHYITEMSTATE.HIS_Expanded;
            }
        }

        /// <summary>
        /// Gets the kind of node.
        /// </summary>
        public virtual SolutionNodeKind Kind { get; private set; }

        /// <summary>
        /// Gets the child nodes.
        /// </summary>
        public virtual IEnumerable<ISolutionExplorerNode> Nodes
        {
            get
            {
                return this.hierarchyNode.Children
                    .Select(node => CreateNode(node))
                    // Skip null nodes which is what the factory may return if the hierarchy 
                    // node is unsupported.
                    .OfType<ISolutionExplorerNode>()
                    .Where(n => n != null);
            }
        }

        /// <summary>
        /// Tries to smart-cast this node to the give type.
        /// </summary>
        /// <typeparam name="T">Type to smart-cast to.</typeparam>
        /// <returns>
        /// The casted value or null if it cannot be converted to that type.
        /// </returns>
        public abstract T As<T>() where T : class;

        /// <summary>
        /// Collapses this node.
        /// </summary>
        public virtual void Collapse()
        {
            foreach (var child in this.Nodes)
            {
                child.Collapse();
            }

            ErrorHandler.ThrowOnFailure(this.window.Value.ExpandItem(
                this.hierarchyNode.VsHierarchy as IVsUIHierarchy, this.hierarchyNode.ItemId, EXPANDFLAGS.EXPF_CollapseFolder));
        }

        /// <summary>
        /// Expands the node, optionally in a recursive fashion.
        /// </summary>
        /// <param name="recursively">if set to <c>true</c>, expands recursively</param>
        public virtual void Expand(bool recursively = false)
        {
            ErrorHandler.ThrowOnFailure(this.window.Value.ExpandItem(
                this.hierarchyNode.VsHierarchy as IVsUIHierarchy, this.hierarchyNode.ItemId, EXPANDFLAGS.EXPF_ExpandParentsToShowItem));

            var flags = EXPANDFLAGS.EXPF_ExpandFolder;
            if (recursively)
                flags |= EXPANDFLAGS.EXPF_ExpandFolderRecursively;

            ErrorHandler.ThrowOnFailure(this.window.Value.ExpandItem(
                this.hierarchyNode.VsHierarchy as IVsUIHierarchy, this.hierarchyNode.ItemId, flags));
        }

        /// <summary>
        /// Selects the node, optionally allowing multiple selection.
        /// </summary>
        /// <param name="allowMultiple">if set to <c>true</c>, adds this node to the current selection.</param>
        /// <exception cref="System.NotSupportedException"></exception>
        public virtual void Select(bool allowMultiple = false)
        {
            var flags = allowMultiple ? EXPANDFLAGS.EXPF_AddSelectItem : EXPANDFLAGS.EXPF_SelectItem;

            var hr = this.window.Value.ExpandItem(
                this.hierarchyNode.VsHierarchy as IVsUIHierarchy, this.hierarchyNode.ItemId, flags);

            if (!ErrorHandler.Succeeded(hr))
            {
                // Workaround for virtual nodes.
                var dte = this.hierarchyNode.ServiceProvider.GetService<DTE>();
                dynamic window = dte.Windows.Item(EnvDTE.Constants.vsWindowKindSolutionExplorer).Object;
                var selectionType = allowMultiple ? vsUISelectionType.vsUISelectionTypeToggle : vsUISelectionType.vsUISelectionTypeSelect;

                var path = this.DisplayName;
                var current = this.Parent;
                while (current != null)
                {
                    path = Path.Combine(current.DisplayName, path);
                    current = current.Parent;
                }

                try
                {
                    var item = window.GetItem(path);
                    if (item != null)
                        item.Select(selectionType);
                }
                catch (Exception)
                {
                    throw new NotSupportedException(Strings.SolutionTreeNode.SelectionUnsupported(path));
                }
            }
        }

        /// <summary>
        /// Accepts the specified visitor for traversal.
        /// </summary>
        /// <returns>
        ///     <see langword="true" /> if the operation should continue with other sibling or child nodes; <see langword="false" /> otherwise.
        /// </returns>
        public abstract bool Accept(ISolutionVisitor visitor);

        /// <summary>
        /// Creates a new child node using the node factory received in the constructor.
        /// </summary>
        /// <param name="hierarchyNode">The hierarchy node to create the child node for.</param>
        /// <remarks>
        /// This method is used for example when the node exposes child node creation APIs, such as
        /// the <see cref="FolderNode"/>.
        /// </remarks>
        protected virtual ITreeNode CreateNode(IVsSolutionHierarchyNode hierarchyNode)
        {
            return this.factory.CreateNode(new Lazy<ITreeNode>(() => this), hierarchyNode);
        }

        /// <summary>
        /// Gets the child nodes.
        /// </summary>
        IEnumerable<ITreeNode> ITreeNode.Nodes
        {
            get { return Nodes; }
        }

        private static T GetProperty<T>(IVsHierarchy hierarchy, __VSHPROPID propId, uint itemid)
        {
            object value = null;
            int hr = hierarchy.GetProperty(itemid, (int)propId, out value);
            if (hr != VSConstants.S_OK || value == null)
            {
                return default(T);
            }
            return (T)value;
        }

        private IVsUIHierarchyWindow GetWindow(IServiceProvider serviceProvider)
        {
            var uiShell = serviceProvider.GetService<SVsUIShell, IVsUIShell>();
            object pvar = null;
            IVsWindowFrame frame;
            var persistenceSlot = new Guid(EnvDTE.Constants.vsWindowKindSolutionExplorer);
            if (ErrorHandler.Succeeded(uiShell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fForceCreate, ref persistenceSlot, out frame)))
                ErrorHandler.ThrowOnFailure(frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out pvar));

            return (IVsUIHierarchyWindow)pvar;
        }

        private string BuildDebuggerDisplay()
        {
            return UIThread.Default.Invoke<string>(() =>
            {
                var display = this.DisplayName;
                var current = this.parent.Value;
                while (current != null)
                {
                    if (current != null)
                        display = current.DisplayName + "\\" + display;

                    current = current.Parent;
                }

                display = "DisplayName = " + display;

                var service = this.adapter as AdapterService;
                if (service != null)
                {
                    var conversions = service.GetSupportedConversions(this.GetType())
                        .Select(type => type.FullName)
                        .Distinct()
                        .OrderBy(s => s)
                        .ToList();

                    if (conversions.Count > 0)
                    {
                        display += ", As<T> = " + string.Join(" | ", conversions);
                    }
                }

                return "{ " + display + " }";
            });
        }
    }
}
