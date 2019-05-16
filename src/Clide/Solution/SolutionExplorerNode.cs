using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Clide.Properties;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Internal.VisualStudio.PlatformUI;

namespace Clide
{
    /// <summary>
    /// Default base implementation of <see cref="ISolutionExplorerNode"/>.
    /// </summary>
    [DebuggerDisplay("{Name} ({Kind})")]
    public abstract class SolutionExplorerNode : ISolutionExplorerNode
    {
        protected IVsHierarchyItem hierarchyItem;
        protected ISolutionExplorerNodeFactory nodeFactory;
        protected IAdapterService adapter;

        protected JoinableLazy<IVsUIHierarchyWindow> solutionExplorer;
        Lazy<ISolutionExplorerNode> parent;
        Lazy<string> name;
        Lazy<bool> isHidden;
        JoinableLazy<ISolutionNode> solutionNode;

        IVsHierarchy hierarchy;
        uint itemId;

        /// <summary>
        /// Initializes a new instance of the <see cref="SolutionExplorerNode"/> class.
        /// </summary>
        /// <param name="nodeKind">Kind of the node.</param>
        /// <param name="hierarchyItem">The underlying hierarchy represented by this node.</param>
        /// <param name="nodeFactory">The factory for child nodes.</param>
        /// <param name="adapter">The adapter service that implements the smart cast <see cref="ISolutionExplorerNode.As{T}"/>.</param>
        protected SolutionExplorerNode(
            SolutionNodeKind nodeKind,
            IVsHierarchyItem hierarchyItem,
            ISolutionExplorerNodeFactory nodeFactory,
            IAdapterService adapter,
            JoinableLazy<IVsUIHierarchyWindow> solutionExplorer)
        {
            Guard.NotNull(nameof(hierarchyItem), hierarchyItem);
            Guard.NotNull(nameof(nodeFactory), nodeFactory);
            Guard.NotNull(nameof(adapter), adapter);
            Guard.NotNull(nameof(solutionExplorer), solutionExplorer);

            this.hierarchyItem = hierarchyItem;
            this.nodeFactory = nodeFactory;
            this.adapter = adapter;
            this.solutionExplorer = solutionExplorer;

            Kind = nodeKind;
            parent = hierarchyItem.Parent == null ? new Lazy<ISolutionExplorerNode>(() => null) : new Lazy<ISolutionExplorerNode>(() => nodeFactory.CreateNode(hierarchyItem.Parent));
            name = new Lazy<string>(() => hierarchyItem.GetProperty(VsHierarchyPropID.Name, ""));

            Func<bool> getHiddenProperty = () => this.hierarchyItem.GetProperty(VsHierarchyPropID.IsHiddenItem, false);

            isHidden = hierarchyItem.Parent != null ?
                new Lazy<bool>(() => getHiddenProperty() || parent.Value.IsHidden) :
                new Lazy<bool>(() => getHiddenProperty());

            solutionNode = new JoinableLazy<ISolutionNode>(async () =>
                await ServiceLocator.Global.GetExport<ISolutionExplorer>().Solution);

            if (hierarchyItem.HierarchyIdentity.IsNestedItem)
            {
                hierarchy = hierarchyItem.HierarchyIdentity.NestedHierarchy;
                itemId = hierarchyItem.HierarchyIdentity.NestedItemID;
            }
            else
            {
                hierarchy = hierarchyItem.HierarchyIdentity.Hierarchy;
                itemId = hierarchyItem.HierarchyIdentity.ItemID;
            }
        }

        /// <summary>
        /// Gets the underlying hierarchy node represented by this node.
        /// </summary>
        protected internal IVsHierarchyItem HierarchyNode => hierarchyItem;

        /// <summary>
        /// Gets the hierarchy represented by this node
        /// </summary>
        protected internal virtual IVsHierarchy Hierarchy => hierarchy;

        /// <summary>
        /// Gets the adapter service used to construct this node.
        /// </summary>
        protected IAdapterService Adapter => adapter;

        /// <summary>
        /// Gets the parent of this node.
        /// </summary>
        public virtual ISolutionExplorerNode Parent => parent.Value;

        /// <summary>
        /// Gets the owning solution.
        /// </summary>
        public virtual ISolutionNode OwningSolution => solutionNode.GetValue();

        /// <summary>
        /// Gets the node display name, as returned by the <see cref="VsHierarchyPropID.Name"/> property.
        /// </summary>
        public virtual string Name => name.Value;

        /// <summary>
        /// Gets the node text as shown in the solution explorer window. 
        /// May not be the same as the <see cref="Name"/> (i.e. for the 
        /// solution node itself, it isn't). Returns same value as 
        /// <see cref="IVsHierarchyItem.Text"/>
        /// </summary>
        public virtual string Text => hierarchyItem.Text;

        /// <summary>
        /// Gets a value indicating whether this node is hidden.
        /// </summary>
        public virtual bool IsHidden => isHidden.Value;

        /// <summary>
        /// Gets a value indicating whether this node is visible by the user.
        /// </summary>
        // If node and its parent isn't hidden
        public virtual bool IsVisible => !IsHidden &&
            // And either parent is null or it's visible and expanded
            (Parent == null || (Parent.IsVisible && Parent.IsExpanded));

        /// <summary>
        /// Gets a value indicating whether this node is selected.
        /// </summary>
        public virtual bool IsSelected
        {
            get
            {
                uint state;
                ErrorHandler.ThrowOnFailure(solutionExplorer.GetValue().GetItemState(
                    hierarchy as IVsUIHierarchy, itemId, (uint)__VSHIERARCHYITEMSTATE.HIS_Selected, out state));

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
                if (Parent == null)
                    return true;

                uint state;
                ErrorHandler.ThrowOnFailure(solutionExplorer.GetValue().GetItemState(
                    hierarchy as IVsUIHierarchy, itemId, (uint)__VSHIERARCHYITEMSTATE.HIS_Expanded, out state));

                return state == (uint)__VSHIERARCHYITEMSTATE.HIS_Expanded;
            }
        }

        /// <summary>
        /// Gets the kind of node.
        /// </summary>
        public virtual SolutionNodeKind Kind { get; }

        /// <summary>
        /// Gets the child nodes.
        /// </summary>
        public virtual IEnumerable<ISolutionExplorerNode> Nodes
        {
            get
            {
                var actualHierarchy = hierarchyItem.GetActualHierarchy();

                var childItemId = HierarchyUtilities.GetFirstChild(
                    actualHierarchy,
                    hierarchyItem.GetActualItemId(),
                    true);

                while (childItemId != VSConstants.VSITEMID_NIL)
                {
                    var node = nodeFactory.CreateNode(actualHierarchy, childItemId);

                    if (node != null)
                        yield return node;

                    childItemId = HierarchyUtilities.GetNextSibling(actualHierarchy, childItemId, true);
                }
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
        /// Collapses this node and all its children.
        /// </summary>
        public virtual void Collapse()
        {
            foreach (var child in Nodes)
            {
                child.Collapse();
            }

            ErrorHandler.ThrowOnFailure(solutionExplorer.GetValue().ExpandItem(
                hierarchy as IVsUIHierarchy, itemId, EXPANDFLAGS.EXPF_CollapseFolder));
        }

        /// <summary>
        /// Expands the node, optionally in a recursive fashion.
        /// </summary>
        /// <param name="recursively">if set to <c>true</c>, expands recursively</param>
        public virtual void Expand(bool recursively = false)
        {
            ErrorHandler.ThrowOnFailure(solutionExplorer.GetValue().ExpandItem(
                hierarchy as IVsUIHierarchy, itemId, EXPANDFLAGS.EXPF_ExpandParentsToShowItem));

            var flags = EXPANDFLAGS.EXPF_ExpandFolder;
            if (recursively)
                flags |= EXPANDFLAGS.EXPF_ExpandFolderRecursively;

            ErrorHandler.ThrowOnFailure(solutionExplorer.GetValue().ExpandItem(
                hierarchy as IVsUIHierarchy, itemId, flags));
        }

        /// <summary>
        /// Selects the node, optionally allowing multiple selection.
        /// </summary>
        /// <param name="allowMultiple">if set to <c>true</c>, adds this node to the current selection.</param>
        /// <exception cref="System.NotSupportedException"></exception>
        public virtual void Select(bool allowMultiple = false)
        {
            var flags = allowMultiple ? EXPANDFLAGS.EXPF_AddSelectItem : EXPANDFLAGS.EXPF_SelectItem;

            var hr = solutionExplorer.GetValue().ExpandItem(hierarchy as IVsUIHierarchy, itemId, flags);

            if (!ErrorHandler.Succeeded(hr))
            {
                // Workaround for virtual nodes.
                var dte = hierarchyItem.GetServiceProvider().GetService<DTE>();
                dynamic explorer = dte.Windows.Item(EnvDTE.Constants.vsWindowKindSolutionExplorer).Object;
                var selectionType = allowMultiple ? vsUISelectionType.vsUISelectionTypeToggle : vsUISelectionType.vsUISelectionTypeSelect;

                var path = Name;
                var current = Parent;
                while (current != null)
                {
                    path = Path.Combine(current.Name, path);
                    current = current.Parent;
                }

                try
                {
                    var item = explorer.GetItem(path);
                    if (item != null)
                        item.Select(selectionType);
                }
                catch (Exception)
                {
                    throw new NotSupportedException(Strings.SolutionExplorerNode.SelectionUnsupported(path));
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
        /// <param name="item">The hierarchy node to create the child node for.</param>
        /// <remarks>
        /// This method is used for example when the node exposes child node creation APIs, such as
        /// the <see cref="FolderNode"/>.
        /// </remarks>
        protected virtual ISolutionExplorerNode CreateNode(IVsHierarchyItem item) => nodeFactory.CreateNode(item);

        /// <summary>
        /// Retrieves the value of a property for the current hierarchy item.
        /// </summary>
        protected T GetProperty<T>(int propId, T defaultValue = default(T)) => hierarchyItem.GetProperty(propId, defaultValue);

        #region Equality

        /// <summary>
        /// Gets whether the current node equals the given node.
        /// </summary>
        public bool Equals(ISolutionExplorerNode other) => Equals(this, other as SolutionExplorerNode);

        /// <summary>
        /// Gets whether the current node equals the given node.
        /// </summary>
        public override bool Equals(object obj) => Equals(this, obj as SolutionExplorerNode);

        /// <summary>
        /// Gets whether the given nodes are equal.
        /// </summary>
        public static bool Equals(SolutionExplorerNode node1, SolutionExplorerNode node2)
        {
            if (Object.Equals(null, node1) ||
                Object.Equals(null, node2) ||
                node1.GetType() != node2.GetType())
                return false;

            if (Object.ReferenceEquals(node1, node2)) return true;

            return node1.hierarchyItem.HierarchyIdentity.Equals(node2.hierarchyItem.HierarchyIdentity);
        }

        public override int GetHashCode() => hierarchyItem.HierarchyIdentity.GetHashCode();

        #endregion
    }
}
