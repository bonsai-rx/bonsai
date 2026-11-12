[CmdletBinding()] param (
    [string[]]$docfxArgs
)
Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

Push-Location $PSScriptRoot
try {
    $libPaths = @()
    $libPaths += Get-ChildItem "..\artifacts\bin\*\release_net4*" -Directory | Select-Object -Expand FullName
    $libPaths += "..\artifacts\package\release"

    ./export-images.ps1 $libPaths
    dotnet docfx metadata
    dotnet docfx build $docfxArgs
} finally {
    Pop-Location
}
