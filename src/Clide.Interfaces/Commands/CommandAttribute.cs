using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Clide
{
	[MetadataAttribute]
	public class CommandAttribute : ExportAttribute
	{
		internal const string CommandContractName = "Clide.Command";

		public CommandAttribute(string packageGuid, string groupGuid, int commandId)
			: base(CommandContractName, typeof(ICommandExtension))
		{
			PackageId = packageGuid;
			GroupId = groupGuid;
			CommandId = commandId;
		}

		public string PackageId { get; }

		public string GroupId { get; }

		public int CommandId { get; }
	}
}