# InteractiveSelect

## Installation

```powershell
PS> Install-Module InteractiveSelect -Scope CurrentUser
PS> Import-Module InteractiveSelect
```

## Usage

```powershell
PS> Get-ChildItem | Select-Interactive -Property Name -Preview { Get-ChildItem $_ }
```

See [examples](examples/) folder for more.

## Build and run locally

```powershell
PS> .\scripts\BuildRelease.ps1
PS> Import-Module .\artifacts\publish\InteractiveSelect\InteractiveSelect.psd1
```
