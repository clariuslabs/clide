using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clide
{
	static partial class ContractNames
	{
		public partial class Interop
		{
			const string Prefix = ContractNames.Prefix + "Interop.";

			public const string SolutionExplorerWindow = Prefix + "SolutionExplorerWindow";
			public const string SolutionExplorerSelection = Prefix + "SolutionExplorerSelection";
			public const string IVsHierarchyItemManager = Prefix + "IVsHierarchyItemManager";
		}
	}
}
