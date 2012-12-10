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
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;
    using Clide.Patterns.Adapter;
    using Clide.VisualStudio;

    internal class SolutionTreeNode : ITreeNode
	{
		private IVsSolutionHierarchyNode hierarchyNode;
		private ITreeNodeFactory<IVsSolutionHierarchyNode> factory;
		private IAdapterService adapter;
		private Lazy<IVsUIHierarchyWindow> window;
		private Lazy<ITreeNode> parent;

		public SolutionTreeNode(
			SolutionNodeKind nodeKind,
			IVsSolutionHierarchyNode hierarchyNode,
			Lazy<ITreeNode> parentNode,
			ITreeNodeFactory<IVsSolutionHierarchyNode> nodeFactory,
			IAdapterService adapter)
		{
			this.hierarchyNode = hierarchyNode;
			this.factory = nodeFactory;
			this.adapter = adapter;
			this.window = new Lazy<IVsUIHierarchyWindow>(() => GetWindow(this.hierarchyNode.ServiceProvider));
			this.parent = parentNode ?? new Lazy<ITreeNode>(() => null);
			this.DisplayName = this.hierarchyNode.VsHierarchy.Properties(hierarchyNode.ItemId).DisplayName;
            this.Kind = nodeKind;
		}

		protected internal IVsSolutionHierarchyNode HierarchyNode { get { return this.hierarchyNode; } }

		public ITreeNode Parent { get { return this.parent.Value; } }

		public string DisplayName { get; private set; }

		public virtual bool IsVisible
		{
			get
			{
				if (this.Parent == null)
					return true;

				return this.Parent == null ||
					(this.Parent.IsVisible && this.Parent.IsExpanded);
			}
		}

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

        public virtual SolutionNodeKind Kind { get; private set; }

		public virtual IEnumerable<ITreeNode> Nodes
		{
			get
			{
				return this.hierarchyNode.Children
					.Select(node => CreateNode(node))
					// Skip null nodes which is what the factory may return if the hierarchy 
					// node is unsupported.
					.Where(n => n != null);
			}
		}

		protected virtual ITreeNode CreateNode(IVsSolutionHierarchyNode hierarchyNode)
		{
			return this.factory.CreateNode(new Lazy<ITreeNode>(() => this), hierarchyNode);
		}

		public virtual T As<T>() where T : class
		{
            if (typeof(T) == typeof(IVsHierarchy))
                return (T)this.hierarchyNode.VsHierarchy;
            else if (typeof(T) == typeof(IVsSolutionHierarchyNode))
                return (T)this.hierarchyNode;

			return this.adapter.As<T>(this);
		}

        public virtual void Collapse()
		{
			foreach (var child in this.Nodes)
			{
				child.Collapse();
			}

			ErrorHandler.ThrowOnFailure(this.window.Value.ExpandItem(
				this.hierarchyNode.VsHierarchy as IVsUIHierarchy, this.hierarchyNode.ItemId, EXPANDFLAGS.EXPF_CollapseFolder));
		}

		public virtual void Expand()
		{
			ErrorHandler.ThrowOnFailure(this.window.Value.ExpandItem(
				this.hierarchyNode.VsHierarchy as IVsUIHierarchy, this.hierarchyNode.ItemId, EXPANDFLAGS.EXPF_ExpandParentsToShowItem));

			ErrorHandler.ThrowOnFailure(this.window.Value.ExpandItem(
				this.hierarchyNode.VsHierarchy as IVsUIHierarchy, this.hierarchyNode.ItemId, EXPANDFLAGS.EXPF_ExpandFolder));
		}

		public virtual void Select(bool allowMultiple = false)
		{
			var flags = allowMultiple ? EXPANDFLAGS.EXPF_ExtendSelectItem : EXPANDFLAGS.EXPF_SelectItem;

			ErrorHandler.ThrowOnFailure(this.window.Value.ExpandItem(
				this.hierarchyNode.VsHierarchy as IVsUIHierarchy, this.hierarchyNode.ItemId, flags));
		}

        public override string ToString()
        {
            var display = this.DisplayName;
            var current = this.parent.Value;
            while (current !=  null)
            {
                if (current != null)
                    display = current.DisplayName + "\\" + display;

                current = current.Parent;
            }

            return display;
        }
		
        private IVsUIHierarchyWindow GetWindow(IServiceProvider serviceProvider)
		{
			IVsWindowFrame frame;
			object obj2;
			IVsUIShell service = serviceProvider.GetService(typeof(SVsUIShell)) as IVsUIShell;
			Guid rguidPersistenceSlot = new Guid("{3AE79031-E1BC-11D0-8F78-00A0C9110057}");
			ErrorHandler.ThrowOnFailure(service.FindToolWindow(0x80000, ref rguidPersistenceSlot, out frame));
			ErrorHandler.ThrowOnFailure(frame.GetProperty(-3001, out obj2));
			return obj2 as IVsUIHierarchyWindow;
		}
	}
}
