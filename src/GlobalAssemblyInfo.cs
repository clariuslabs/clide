#region BSD License
/*
Copyright (c) 2012, Clarius Consulting
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

#pragma warning disable 0436
using System.Reflection;

[assembly: AssemblyProduct("Clide")]
[assembly: AssemblyCompany("Clarius Consulting")]
[assembly: AssemblyCopyright("Copyright 2012")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

#if DEBUG
[assembly: AssemblyConfiguration("DEBUG")]
#else
[assembly: AssemblyConfiguration ("RELEASE")]
#endif

// AssemblyVersion = full version info, since it's used to determine agents versions
[assembly: AssemblyVersion(ThisAssembly.SimpleVersion)]
// FileVersion = release-like simple version (i.e. 3.11.2 for cycle 5, SR2).
[assembly: AssemblyFileVersion(ThisAssembly.FullVersion)]
// InformationalVersion = full version + branch + commit sha.
[assembly: AssemblyInformationalVersion(ThisAssembly.InformationalVersion)]

partial class ThisAssembly
{
    /// <summary>
    /// Simple release-like version number.
    /// </summary>
    public const string SimpleVersion = Git.BaseVersion.Major + "." + Git.BaseVersion.Minor + "." + Git.BaseVersion.Patch;

    /// <summary>
    /// Full version, including commits since base version file, like 4.0.1.598
    /// </summary>
    public const string FullVersion = SimpleVersion + "." + Git.Commits;

    /// <summary>
    /// Full version, plus branch and commit short sha.
    /// </summary>
    public const string InformationalVersion = FullVersion + "-" + Git.Branch + "+" + Git.Commit;
}

#pragma warning restore 0436