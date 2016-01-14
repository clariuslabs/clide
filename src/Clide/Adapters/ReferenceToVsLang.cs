using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using VSLangProj;

namespace Clide
{
	[Export (typeof (IAdapter))]
	class ReferenceToVsLang : IAdapter<ReferenceNode, Reference>
	{
		public Reference Adapt (ReferenceNode from)
		{
			return from.Reference.Value;
		}
	}
}