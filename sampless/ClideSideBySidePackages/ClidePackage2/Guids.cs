// Guids.cs
// MUST match guids.h
using System;

namespace Clarius.ClidePackage2
{
    static class GuidList
    {
        public const string guidClidePackage2PkgString = "dc6bedb3-d23e-4f15-93b8-128d6ead5aa0";
        public const string guidClidePackage2CmdSetString = "f0e56af0-2a34-4adf-85a0-a2b27c25e6a8";

        public static readonly Guid guidClidePackage2CmdSet = new Guid(guidClidePackage2CmdSetString);
    };
}