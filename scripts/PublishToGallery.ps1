#Requires -PSEdition Core -Module PowerShellGet
[CmdletBinding()]
param(
    [Parameter(Mandatory, ParameterSetName = "LocalPublish")]
    [String] $LocalRepositoryName,

    [Parameter(Mandatory, ParameterSetName = "OnlinePublish")]
    [switch] $Online,

    [Parameter(Mandatory, ParameterSetName = "OnlinePublish")]
    [String] $ApiKey
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$solutionPath = Split-Path $PSScriptRoot
$publishPath = Join-Path $solutionPath 'artifacts' 'publish'
$modulePath = Join-Path $publishPath 'InteractiveSelect'
$manifestPath = Join-Path $modulePath 'InteractiveSelect.psd1'

if (-not (Test-Path $manifestPath)) {
    throw "Can't find '$manifestPath'. Run 'BuildRelease.ps1' first"
}

$originalModulePath = $env:PSModulePath
try {
    $env:PSModulePath = "$publishPath;$env:PSModulePath"

    if ($Online) {
        Write-Host "Running 'Publish-Module -WhatIf ...' for '$modulePath'" -ForegroundColor Cyan
        Publish-Module `
            -Path $ModulePath `
            -Repository PSGallery `
            -NuGetApiKey $ApiKey `
            -Verbose `
            -WhatIf `
            -ErrorAction Stop

        if ($PSCmdlet.ShouldContinue("Publish module '$modulePath' to PSGallery?", "Confirm Publish")) {
            Publish-Module `
                -Path $modulePath `
                -Repository PSGallery `
                -NuGetApiKey $ApiKey `
                -ErrorAction Stop

            Write-Host 'Publish succeeded' -ForegroundColor Green
        }
        else {
            Write-Host 'Publish was cancelled' -ForegroundColor Yellow
        }
    }
    else {
        Write-Host "Running local publish to repository '$LocalRepositoryName'"

        Publish-Module `
            -Path $modulePath `
            -Repository $LocalRepositoryName `
            -ErrorAction Stop

        Write-Host 'Publish succeeded' -ForegroundColor Green
    }
}
finally {
    $env:PSModulePath = $originalModulePath
}
