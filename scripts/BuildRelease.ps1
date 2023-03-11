[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$rootPath = Split-Path $PSScriptRoot
$artifactsPath = Join-Path $rootPath artifacts
$publishPath = Join-Path $artifactsPath publish
$projectPath = Join-Path $rootPath src InteractiveSelect.csproj

if (Test-Path $publishPath) {
    Write-Host "Removing $publishPath"
    Remove-Item -LiteralPath $publishPath -Force -Recurse
}

$modulePath = Join-Path $publishPath InteractiveSelect
dotnet publish $projectPath `
    --configuration Release `
    --output $modulePath `
    --verbosity minimal

if ($LASTEXITCODE -ne 0) {
    throw "dotnet build exited with error code $LASTEXITCODE"
}

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

$expectedLicenseUrl = "$($manifestInfo.ProjectUri)/blob/v$expectedVersion/LICENSE.txt"
if ($manifestInfo.LicenseUri -ne $expectedLicenseUrl) {
    throw "License URI '$($manifestInfo.LicenseUri)' does not match expected value '$expectedLicenseUrl'"
}

if ($manifestInfo.ReleaseNotes -notlike "v$expectedVersion*") {
    throw "Release notes should be updated in psd1 file"
}

Write-Host
Write-Host "Module published to: $modulePath" -ForegroundColor Green
