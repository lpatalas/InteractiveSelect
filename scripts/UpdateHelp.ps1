#Requires -Module platyPS
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$solutionDir = Split-Path $PSScriptRoot
$helpPath = Join-Path $solutionDir 'help'

Update-MarkdownHelp -Path $helpPath
