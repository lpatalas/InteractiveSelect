$DebugPreference = 'Continue'
Import-Module "$PSScriptRoot\InteractiveSelect.dll"

$emptyArray = @()
$nullArray = $null
$stringArray = 'a', 'b', 'c'
$arrayWithNull = 'a', $null, 'c'
