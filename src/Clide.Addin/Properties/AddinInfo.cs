using System;
using Mono.Addins;
using Mono.Addins.Description;

[assembly: Addin(
    "Clide.Addin",
    Version =
        ThisAssembly.Git.SemVer.Major + "." +
        ThisAssembly.Git.SemVer.Minor + "." +
        ThisAssembly.Git.SemVer.Patch,
    Flags = AddinFlags.Hidden
)]

[assembly: AddinName("Clide.Addin")]
[assembly: AddinCategory("IDE extensions")]
[assembly: AddinDescription("Clide.Addin")]
[assembly: AddinAuthor("Xamarin")]
