using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Clide
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CommandAttribute : ExportAttribute
    {
        public CommandAttribute(string packageGuid, string groupGuid, int commandId)
            : base(typeof(ICommandExtension))
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