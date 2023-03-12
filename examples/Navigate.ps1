[CmdletBinding(SupportsShouldProcess)]
param()

function NewItem($label, $item) {
    [pscustomobject]@{
        Label = $label
        Item = $item
    }
}

$currentDir = Get-Item (Get-Location)
$done = $false

while (-not $done) {
    $items = @()
    if ($currentDir.Parent) {
        $items += @([pscustomobject]@{
            Action = 'GoToParent'
            Label = '..'
            Item = $currentDir
        })
    }

    $items += @(Get-ChildItem -Directory $currentDir | ForEach-Object { NewItem $_.Name $_ })

    $result = Select-Interactive `
        -Items $items `
        -Property Label `
        -Preview {
            Get-ChildItem $_.Item | Format-Table | Out-String
        } `
        -KeyBindings @{
            'Control+Enter' = {
                param($api)
                $api.Exit([pscustomobject]@{
                    Action = 'Accept'
                    Item = $api.HighlightedValue.Item
                })
            }
            'Control+Backspace' = {
                param($api)
                $api.Exit([pscustomobject]@{
                    Action = 'GoToParent'
                    Item = $api.HighlightedValue.Item
                })
            }
        }

    if ($result) {
        if ($result.Action -eq 'Accept') {
            $selectedPath = $result.Item.FullName

            if ($PSCmdlet.ShouldProcess($selectedPath, 'Set-Location')) {
                Set-Location $selectedPath
            }
            $done = $true
        }
        elseif ($result.Action -eq 'GoToParent') {
            if ($currentDir.Parent) {
                $currentDir = $currentDir.Parent
            }
        }
        else {
            $currentDir = $result.Item
        }
    }
    else {
        $done = $true
    }
}
