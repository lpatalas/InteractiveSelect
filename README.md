# InteractiveSelect

## Installation

```powershell
Install-Module InteractiveSelect -Scope CurrentUser
Import-Module InteractiveSelect
```

## Usage

```powershell
Get-ChildItem | Select-Interactive -Property Name -Preview { Get-ChildItem $_ }
```
