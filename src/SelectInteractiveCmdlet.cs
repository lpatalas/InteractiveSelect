using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Internal;

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
    public ScriptBlock? FormatLabel { get; set; }

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
            try
            {
                Console.CursorVisible = false;
                var listView = new ListView(listItems, 10);
                var results = listView.SelectItems(Host.UI);
                WriteObject(results, enumerateCollection: true);
            }
            finally
            {
                Console.CursorVisible = true;
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

    private string GetLabelForItem(object? item, int itemIndex)
    {
        string GetDefaultLabel()
            => item?.ToString() ?? string.Empty;

        if (FormatLabel is not null)
        {
            try
            {
                var result = FormatLabel.Invoke(item);
                if (result.Count > 0 && result[0] is not null)
                    return result[0].ToString();
                else
                    return GetDefaultLabel();
            }
            catch (RuntimeException ex)
            {
                WriteError(new ErrorRecord(
                    new ItemCallbackScriptException(nameof(FormatLabel), itemIndex, ex),
                    "SelectInteractive_FormatLabel_RuntimeException",
                    ErrorCategory.InvalidOperation,
                    item));
                return GetDefaultLabel();
            }
        }
        else
        {
            return GetDefaultLabel();
        }
    }
}
