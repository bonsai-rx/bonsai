Push-Location $PSScriptRoot
if (!(Test-Path "./Bonsai.exe")) {
    $release = "https://github.com/bonsai-rx/bonsai/releases/latest/download/Bonsai.zip"
    $configPath = "./Bonsai.config"
    if (Test-Path $configPath) {
        [xml]$config = Get-Content $configPath
        $bootstrapper = $config.PackageConfiguration.Packages.Package.where{$_.id -eq 'Bonsai'}
        if ($bootstrapper) {
            $version = $bootstrapper.version
            $release = "https://github.com/bonsai-rx/bonsai/releases/download/$version/Bonsai.zip"
        }
    }
    Invoke-WebRequest $release -OutFile "temp.zip"
    Move-Item -Path "NuGet.config" "temp.config" -ErrorAction SilentlyContinue
    Expand-Archive "temp.zip" -DestinationPath "." -Force
    Move-Item -Path "temp.config" "NuGet.config" -Force -ErrorAction SilentlyContinue
    Remove-Item -Path "temp.zip"
    Remove-Item -Path "Bonsai32.exe"
}
& .\Bonsai.exe --no-editor
Pop-Location