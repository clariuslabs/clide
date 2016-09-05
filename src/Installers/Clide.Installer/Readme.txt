This NuGet package targets VSIX projects that embed the Clide VSIX or WiX Toolset 
projects (.wixproj) that build MSIs or chained installers (with Burn).

It provides the installer for the Clide VSIX that provides the MEF components 
to consume Clide services from Visual Studio 2012 or later.

== VSIX Projects ==

If your project's output is a VSIX, installing this NuGet package will automatically 
add the Clide.vsix file as a Content item with CopyToOutputDirectory=PreserveNewest and 
IncludeInVsix=true, as well as add it as an embedded VSIX dependency, like so:

  <Dependency d:Source="File" DisplayName="Clide" Id="Clide" Version="[3.0.0,)"
              d:InstallSource="Embed" Location="Clide.vsix" />

It will also do so with Merq, which is a direct dependency of Clide.

== MSI Projects ==

If your project's output is an MSI (OutputType=Package), installing this NuGet 
package will add the necessary WixVSExtension reference and WixLibrary reference.

However, you must define the following minimal requirements in order to get it 
properly bundled and installed by your MSI:

1 - Your MSI must define these directories to satisfy symbol references in the .wixlib:

    <Fragment>
      <Directory Id="TARGETDIR" Name="SourceDir">
        <Directory Id="ProgramFilesFolder">
          <Directory Id="CommonFilesFolder" />
        </Directory>
      </Directory>
    </Fragment>


	Alternatively, you can reference an already declared TARGETDIR:

    <Fragment>
      <DirectoryRef Id="TARGETDIR">
        <Directory Id="ProgramFilesFolder">
          <Directory Id="CommonFilesFolder" />
        </Directory>
      </Directory>
    </Fragment>


You must also reference this component as part of one of your features:

    <ComponentRef Id="Clide" />

Since Clide depends son Merq, you also need to add it to one of your features:

    <ComponentRef Id="Merq" />


== Burn/Chained installer Projects ==

If your project's output is an chained installer bundle (OutputType=Bundle), you 
just need to reference the included MSI as follows from your Chain:

	<Chain>
      <MsiPackage SourceFile="$(var.Clide.Msi)" />
	  <!-- And likewise for Merq -->
      <MsiPackage SourceFile="$(var.Merq.Msi)" />
	</Chain>
