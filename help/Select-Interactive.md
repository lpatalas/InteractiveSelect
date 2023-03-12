---
external help file: InteractiveSelect.dll-Help.xml
Module Name: InteractiveSelect
online version: https://github.com/lpatalas/InteractiveSelect/blob/v0.9.1/help/Select-Interactive.md
schema: 2.0.0
---

# Select-Interactive

## SYNOPSIS

Filter and select items interactively.

## SYNTAX

### InputFromItems (Default)
```
Select-Interactive [-Property <PSPropertyExpression>] [-Preview <PSPropertyExpression>] [-Items] <PSObject[]>
 [-Height <DimensionParameter>] [-SplitOffset <DimensionParameter>] [-Vertical] [-KeyBindings <KeyBindings>]
 [<CommonParameters>]
```

### InputFromPipeline
```
Select-Interactive [-Property <PSPropertyExpression>] [-Preview <PSPropertyExpression>] -InputObject <PSObject>
 [-Height <DimensionParameter>] [-SplitOffset <DimensionParameter>] [-Vertical] [-KeyBindings <KeyBindings>]
 [<CommonParameters>]
```

## DESCRIPTION

Filter and select items interactively.

## EXAMPLES

### Example 1
```powershell
PS C:\> Get-ChildItem | Select-Interactive -Property Name | Remove-Item
```

Select items from current directory and delete them.

### Example 2
```powershell
PS C:\> Get-Process | Select-Interactive -Property Name | Stop-Process
```

Kill selected processes.

## PARAMETERS

### -Height

Height of the displayed UI. If it is an absolute number (e.g. `50`) then the UI will
take the exact number of rows. If it's a percentage (e.g. `25%`) then the height will
be calculated based on total window height.

```yaml
Type: DimensionParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InputObject

Input objects that will be presented in the list. `-Property` parameter controls the text
that will be presented for each item.

```yaml
Type: PSObject
Parameter Sets: InputFromPipeline
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -Items

Input objects that will be presented in the list. `-Property` parameter controls the text
that will be presented for each item.

```yaml
Type: PSObject[]
Parameter Sets: InputFromItems
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -KeyBindings

Custom key binding that allow the user to define custom actions.

```yaml
Type: KeyBindings
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Preview

Property name or script block that defines what is shown in the preview pane.
If not specified then the preview pane won't be shown at all.

```yaml
Type: PSPropertyExpression
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Property

Property or script block that returns text displayed in item list.
If not specified then `ToString()` method is called on each item to get the text.

```yaml
Type: PSPropertyExpression
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SplitOffset

Controls the sizes of list and preview panes. If it's an absolute value (e.g. `50`) then
list pane will take this exact size. If it's a percentage (e.g. `25%`) then list pane will
take given percentage of total UI size and preview will take the rest.

By default list pane width matches the length of the longest item but not more than 50%.

```yaml
Type: DimensionParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Vertical

If set then panes are displayed vertically - list on top, preview on bottom.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Management.Automation.PSObject

Any object can be piped into the command.

## OUTPUTS

### System.Object

Returns selected object from the input collection.

## NOTES

## RELATED LINKS

[GitHub](https://github.com/lpatalas/InteractiveSelect)
