using Clide;
using Clide.CommonComposition;
using Clide.Sdk.Solution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationPackage
{
    //[Named("SolutionExplorer")]
    //[TreeNodeFactoryAttribute(false)]
    public class CustomProjectNodeFactory : ITreeNodeFactory<IVsSolutionHierarchyNode>
    {
        public CustomProjectNodeFactory()
        {
        }

        public bool Supports(IVsSolutionHierarchyNode hierarchy)
        {
            return true;
        }

        public ITreeNode CreateNode(Lazy<ITreeNode> parent, IVsSolutionHierarchyNode hierarchy)
        {
            return null;
        }
    }
}
