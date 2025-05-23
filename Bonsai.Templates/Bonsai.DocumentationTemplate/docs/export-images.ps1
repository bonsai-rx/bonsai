[CmdletBinding()] param (
    [string[]]$LibrarySources,
    [string]$OutputFolder=$null
)
Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

if ($OutputFolder) {
    $OutputFolder = Join-Path (Get-Location) $OutputFolder
}

function Export-Workflow-Collection([string[]]$libPath, [string]$workflowPath, [string]$environmentPath) {
    $bootstrapperPath = (Join-Path $environmentPath 'Bonsai.exe')
    .\bonsai-docfx\modules\Export-Image.ps1 -libPath $libPath -workflowPath $workflowPath -bootstrapperPath $bootstrapperPath -outputFolder $OutputFolder -documentationRoot $PSScriptRoot
}

Push-Location $PSScriptRoot
try {
    if (Test-Path -Path 'workflows/') {
        Export-Workflow-Collection $LibrarySources './workflows' '../.bonsai/'
    }
} finally {
    Pop-Location
}