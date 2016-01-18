using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using VSLangProj;

namespace Clide
{
	[Adapter]
	class ReferenceToVsLang : IAdapter<ReferenceNode, Reference>
	{
		public Reference Adapt (ReferenceNode from) => from.Reference.Value;
	}
}