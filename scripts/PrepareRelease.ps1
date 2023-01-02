Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$rootPath = Split-Path $PSScriptRoot
$artifactsPath = Join-Path $rootPath artifacts
$publishPath = Join-Path $artifactsPath prepared-release
$projectPath = Join-Path $rootPath src InteractiveSelect.csproj

if (Test-Path $publishPath) {
    Write-Host "Removing $publishPath"
    Remove-Item -LiteralPath $publishPath -Force -Recurse
}

$dotnetPublishPath = Join-Path $publishPath temp
dotnet publish $projectPath `
    --configuration Release `
    --output $dotnetPublishPath `
    --verbosity minimal

if ($LASTEXITCODE -ne 0) {
    throw "dotnet build exited with error code $LASTEXITCODE"
}

$modulePath = Join-Path $publishPath InteractiveSelect
New-Item -Path $modulePath -ItemType Directory | Out-Null

Copy-Item `
    -LiteralPath (Join-Path $dotnetPublishPath InteractiveSelect.dll) `
    -Destination $modulePath

$manifestProperties = & "$rootPath\ModuleManifest.ps1"
$manifestPath = Join-Path $modulePath InteractiveSelect.psd1
New-ModuleManifest `
    -Path $manifestPath `
    @manifestProperties

Test-ModuleManifest -Path $manifestPath

Write-Host
Write-Host "Module is ready in $modulePath" -ForegroundColor Green
