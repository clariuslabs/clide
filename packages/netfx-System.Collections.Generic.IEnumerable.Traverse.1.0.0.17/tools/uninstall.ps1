param($installPath, $toolsPath, $package, $project)

    # remove code-snippets if any
	gci $toolsPath\*.snippet | %{ remove-item (([System.Environment]::ExpandEnvironmentVariables("%VisualStudioDir%\Code Snippets\Visual C#\My Code Snippets\")) + $_.Name) -ErrorAction SilentlyContinue }