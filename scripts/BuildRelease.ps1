#Requires -Module platyPS
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$script:HasErrors = $false

function LogError($message) {
    $script:HasErrors = $true
    Write-Error -Message $message -ErrorAction Continue
}

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
    LogError "Module version in psd1 ($($manifestInfo.Version)) does not match Version.props ($expectedVersion)"
}

$expectedLicenseUrl = "$($manifestInfo.ProjectUri)/blob/v$expectedVersion/LICENSE.txt"
if ($manifestInfo.LicenseUri -ne $expectedLicenseUrl) {
    LogError "License URI '$($manifestInfo.LicenseUri)' does not match expected value '$expectedLicenseUrl'"
}

if ($manifestInfo.ReleaseNotes -notlike "v$expectedVersion*") {
    LogError "Release notes should be updated in psd1 file"
}

$helpSourcePath = Join-Path $rootPath 'help'
Get-ChildItem $helpSourcePath `
| Foreach-Object {
    $onlineVersion = (Select-String -Path $_ -Pattern '^online version:').Line
    $expectedHelpUri = "$($manifestInfo.ProjectUri)/blob/v$expectedVersion/help/$($_.Name)"
    if ($onlineVersion -ne "online version: $expectedHelpUri") {
        LogError "Online URL in '$_' does not match the current version and/or file name '$onlineVersion'. Expected: '$expectedHelpUri'"
    }
}

if ($script:HasErrors) {
    throw "Failing script because of errors"
}

New-ExternalHelp -Path $helpSourcePath -OutputPath $modulePath | Out-Null

Write-Host "Module published to: $modulePath" -ForegroundColor Green
