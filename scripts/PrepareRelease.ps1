[CmdletBinding()]
param()

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
    -Path (Join-Path $dotnetPublishPath '*') `
    -Destination $modulePath `
    -Include InteractiveSelect.dll, InteractiveSelect.psd1

$manifestInfo = Test-ModuleManifest -Path (Join-Path $modulePath InteractiveSelect.psd1)
$manifestInfo | Format-List

$versionProps = [xml](Get-Content (Join-Path $rootPath Version.props))
$expectedVersion = $versionProps.Project.PropertyGroup.PSModuleVersion
if (-not $expectedVersion) {
    throw "Can't get PSModuleVersion from Version.props"
}

if ($expectedVersion -ne $manifestInfo.Version) {
    throw "Module version in psd1 ($($manifestInfo.Version)) does not match Version.props ($expectedVersion)"
}

Write-Host
Write-Host "Module is ready in $modulePath" -ForegroundColor Green
