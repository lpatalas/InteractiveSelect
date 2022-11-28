using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Runtime.InteropServices;
using Microsoft.PowerShell.Commands;

namespace InteractiveSelect;

file static class ParameterSets
{
    public const string InputFromPipeline = nameof(InputFromPipeline);
    public const string InputFromItems = nameof(InputFromItems);
}

[Cmdlet(VerbsCommon.Select, "Interactive", DefaultParameterSetName = ParameterSets.InputFromItems)]
public class SelectInteractiveCmdlet : PSCmdlet
{
    [Parameter]
    public PSPropertyExpression? ItemText { get; set; }

    [Parameter(
        Mandatory = true,
        ParameterSetName = ParameterSets.InputFromPipeline,
        ValueFromPipeline = true)]
    [AllowNull]
    public PSObject? InputObject { get; set; }

    [Parameter(
        Mandatory = true,
        ParameterSetName = ParameterSets.InputFromItems,
        Position = 0)]
    [AllowNull]
    public PSObject?[]? Items { get; set; }

    private bool HasPipelineInput
        => string.Equals(ParameterSetName, ParameterSets.InputFromPipeline, StringComparison.Ordinal);

    protected override void BeginProcessing()
    {
        WriteDebug($"ParameterSetName = {ParameterSetName}");
        WriteDebug($"HasPipelineInput = {HasPipelineInput}");
    }

    protected override void ProcessRecord()
    {
        if (HasPipelineInput)
            pipedItems.Add(CreateListItem(InputObject, pipedItems.Count));
    }

    protected override void EndProcessing()
    {
        var listItems = HasPipelineInput switch
        {
            true => pipedItems,
            false when Items is not null => CreateListItemCollection(Items),
            _ => Array.Empty<ListItem>()
        };
        
        if (listItems.Count > 0)
        {
            // CursorVisible is only available on Windows
            bool? initialCursorVisibility = OperatingSystem.IsWindows() ? Console.CursorVisible : null;

            try
            {
                Console.CursorVisible = false;
                var listView = new ListView(listItems, 10);
                var results = listView.SelectItems(Host.UI);
                WriteObject(results, enumerateCollection: true);
            }
            finally
            {
                if (initialCursorVisibility.HasValue)
                    Console.CursorVisible = initialCursorVisibility.Value;
            }
        }
        else
        {
            WriteDebug("Item array is empty");
        }
    }

    private readonly List<ListItem> pipedItems = new();

    private IReadOnlyList<ListItem> CreateListItemCollection(IReadOnlyList<PSObject?> inputItems)
    {
        var result = new List<ListItem>(inputItems.Count);
        for (int i = 0; i < inputItems.Count; i++)
            result.Add(CreateListItem(inputItems[i], i));
        return result;
    }

    private ListItem CreateListItem(PSObject? inputItem, int itemIndex)
    {
        var label = GetLabelForItem(inputItem, itemIndex);
        return new ListItem(label, inputItem);
    }

    private string GetLabelForItem(PSObject? item, int itemIndex)
    {
        string GetDefaultLabel()
            => item?.ToString() ?? $"(null #{itemIndex})";

        if (ItemText is not null)
        {
            var result = ItemText.GetValues(item);
            if (result is [var firstResult, ..])
            {
                if (firstResult.Result is not null)
                {
                    var labelText = firstResult.Result.ToString();
                    if (!string.IsNullOrEmpty(labelText))
                        return labelText;
                }
                else if (firstResult.Exception is not null)
                {
                    WriteError(new ErrorRecord(
                        new ItemCallbackScriptException(nameof(ItemText), itemIndex, firstResult.Exception),
                        "SelectInteractive_FormatLabel_RuntimeException",
                        ErrorCategory.InvalidOperation,
                        item));
                }
            }
        }

        return GetDefaultLabel();
    }
}
