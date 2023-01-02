#Requires -PSEdition Core -Module PowerShellGet
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateScript({ Test-Path -LiteralPath $_ -PathType Container }, ErrorMessage = 'Directory "{0}" does not exist')]
    [String] $ModulePath,

    [Parameter(Mandatory, ParameterSetName = "LocalPublish")]
    [String] $LocalRepositoryName,

    [Parameter(Mandatory, ParameterSetName = "OnlinePublish")]
    [switch] $Online,

    [Parameter(Mandatory, ParameterSetName = "OnlinePublish")]
    [String] $ApiKey
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not (Test-Path $ModulePath)) {
    throw "ModulePath '$ModulePath' does not exist"
}

$originalModulePath = $env:PSModulePath
try {
    $tempModulesPath = Split-Path $ModulePath
    $env:PSModulePath += ";$tempModulesPath"

    if ($Online) {
        Write-Host "Running 'Publish-Module -WhatIf ...' for '$ModulePath'" -ForegroundColor Cyan
        Publish-Module `
            -Path $ModulePath `
            -Repository PSGallery `
            -NuGetApiKey $ApiKey `
            -Verbose `
            -WhatIf `
            -ErrorAction Stop

        if ($PSCmdlet.ShouldContinue("Publish module '$ModulePath' to PSGallery?", "Confirm Publish")) {
            Publish-Module `
                -Path $ModulePath `
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
            -Path $ModulePath `
            -Repository $LocalRepositoryName `
            -ErrorAction Stop

        Write-Host 'Publish succeeded' -ForegroundColor Green
    }
}
finally {
    $env:PSModulePath = $originalModulePath
}
