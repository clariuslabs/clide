using System;
using Mono.Addins;
using Mono.Addins.Description;

[assembly: Addin(
    "Clide.Addin",
    Namespace = "Clide.Addin",
    Version = "1.0",
    Flags = AddinFlags.Hidden
)]

[assembly: AddinName("Clide.Addin")]
[assembly: AddinCategory("IDE extensions")]
[assembly: AddinDescription("Clide.Addin")]
[assembly: AddinAuthor("Mikayla Hutchinson")]
