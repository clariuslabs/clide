param($installPath, $toolsPath, $package, $project)

    # add code-snippets if any
	copy-item $toolsPath\*.snippet -destination ([System.Environment]::ExpandEnvironmentVariables("%VisualStudioDir%\Code Snippets\Visual C#\My Code Snippets\")) -ErrorAction SilentlyContinue