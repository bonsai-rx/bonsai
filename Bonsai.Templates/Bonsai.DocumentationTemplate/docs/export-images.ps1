[CmdletBinding()] param (
    [string[]]$LibrarySources,
    [bool]$UseGalleryForWorkflowsDirectory=$false,
    [bool]$UseGalleryForExamplesDirectory=$true,
    [string]$OutputFolder=$null
)
Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

if ($OutputFolder) {
    $OutputFolder = Join-Path (Get-Location) $OutputFolder
}

function Process-Workflow-Collection([bool]$useGallery, [string]$workflowPath, [string]$environmentPath) {
    $libPath = $LibrarySources

    if ($useGallery) {
        $libPath = @()
        $galleryPath = Join-Path $environmentPath 'Gallery'
        $null = New-Item -ItemType Directory -Path $galleryPath -Force
        foreach ($librarySource in $LibrarySources) {
            Get-ChildItem -Path $librarySource -Filter *.nupkg | Copy-Item -Destination $galleryPath
        }
    }

    $bootstrapperPath = (Join-Path $environmentPath 'Bonsai.exe')
    .\bonsai-docfx\scripts\Export-Image.ps1 -libPath $libPath -workflowPath $workflowPath -bootstrapperPath $bootstrapperPath -outputFolder $OutputFolder -documentationRoot $PSScriptRoot
}

Push-Location $PSScriptRoot
try {
    if (Test-Path -Path 'workflows/') {
        Process-Workflow-Collection $UseGalleryForWorkflowsDirectory './workflows' '../.bonsai/'
    }

    if (Test-Path -Path 'examples/') {
        foreach ($environment in (Get-ChildItem -Path 'examples/' -Filter '.bonsai' -Recurse -FollowSymlink -Directory)) {
            Process-Workflow-Collection $UseGalleryForExamplesDirectory ($environment.Parent.FullName) ($environment.FullName)
        }
    }
} finally {
    Pop-Location
}
