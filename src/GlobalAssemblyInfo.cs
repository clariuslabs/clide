#pragma warning disable 0436

using System.Reflection;

[assembly: AssemblyProduct ("Clide")]
[assembly: AssemblyCompany ("Clarius Consulting")]
[assembly: AssemblyCopyright ("Copyright 2012")]
[assembly: AssemblyTrademark ("")]
[assembly: AssemblyCulture ("")]

#if DEBUG
[assembly: AssemblyConfiguration ("DEBUG")]
#else
[assembly: AssemblyConfiguration ("RELEASE")]
#endif

[assembly: AssemblyVersion (ThisAssembly.Version)]
[assembly: AssemblyFileVersion (ThisAssembly.Version)]
[assembly: AssemblyInformationalVersion (ThisAssembly.InformationalVersion)]

partial class ThisAssembly
{
	public const string Version = ThisAssembly.Git.SemVer.Major + "." + ThisAssembly.Git.SemVer.Minor + "." + ThisAssembly.Git.SemVer.Patch;
	public const string InformationalVersion = Version + "-" + Git.Branch + "+" + Git.Commit;
}

#pragma warning restore 0436