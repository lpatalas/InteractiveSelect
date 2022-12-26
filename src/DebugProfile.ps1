$DebugPreference = 'Continue'
Import-Module "$PSScriptRoot\InteractiveSelect.psd1"

$dir = Get-Item $pwd
while ($dir.Parent -and (-not (Test-Path (Join-Path $dir "global.json")))) {
    $dir = $dir.Parent
}

Set-Location $dir

function GenerateArray {
    param($Count)

    $digits = 'zero', 'one', 'two', 'three', 'four', 'five', 'six', 'seven', 'eight', 'nine'

    1..$Count | ForEach-Object {
        $result = @()
        $number = $_

        do {
            $digit = $number % 10
            $result = @($digits[$digit]) + $result
            $number = [math]::Floor($number / 10)
        } while ($number -gt 0)

        $result -join ' '
    }
}

$emptyArray = @()
$nullArray = $null
$stringArray = 'a', 'b', 'c'
$arrayWithNull = 'a', $null, 'c'
