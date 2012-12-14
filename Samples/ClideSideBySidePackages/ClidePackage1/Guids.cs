// Guids.cs
// MUST match guids.h
using System;

namespace Clarius.ClidePackage1
{
    static class GuidList
    {
        public const string guidClidePackage1PkgString = "884af8e9-b970-4d64-b21b-5c0e2447c628";
        public const string guidClidePackage1CmdSetString = "64f98849-e985-4154-85ab-1d9dbfb7f9ae";

        public static readonly Guid guidClidePackage1CmdSet = new Guid(guidClidePackage1CmdSetString);
    };
}