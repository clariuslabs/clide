using Microsoft.VisualStudio.Shell;
using System;

namespace Clide
{
	/// <summary>
	/// Base clas for behavior nodes based on automation objects
	/// </summary>
	/// <typeparam name="TNode"></typeparam>
	/// <typeparam name="TAutomation"></typeparam>
	abstract class BehaviorNode<TNode, TAutomation> where TNode : ISolutionExplorerNode
	{
		public BehaviorNode(TNode node)
		{
			this.Node = node;

			Automation = new Lazy<TAutomation>(() =>
			{
				var value = (TAutomation)HierarchyNode.Value.GetExtenderObject();

				if (value == null)
					throw new NotSupportedException();

				return value;
			});
		}

		public Lazy<TAutomation> Automation { get; set; }

		public TNode Node { get; private set; }

		protected abstract Lazy<IVsHierarchyItem> HierarchyNode { get; }
	}
}