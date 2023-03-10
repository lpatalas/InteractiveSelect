using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;

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
    public PSPropertyExpression? Property { get; set; }

    [Parameter]
    public PSPropertyExpression? Preview { get; set; }

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

    [Parameter]
    public DimensionParameter? Height { get; set; }

    [Parameter]
    public SwitchParameter Vertical { get; set; }

    [Parameter]
    public KeyBindings? KeyBindings { get; set; }

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
            pipedObjects.Add(CreateInputObject(InputObject, pipedObjects.Count));
    }

    protected override void EndProcessing()
    {
        var inputObjects = HasPipelineInput switch
        {
            true => pipedObjects,
            false when Items is not null => CreateInputObjectCollection(Items),
            _ => Array.Empty<InputObject>()
        };
        
        if (inputObjects.Count > 0)
        {
            bool didHideCursor = false;

            if (Host.UI.SupportsVirtualTerminal)
            {
                WriteDebug("Hiding cursor using escape sequence");
                Host.UI.Write(EscapeSequence.HideCursor);
                didHideCursor = true;
            }
            else if (OperatingSystem.IsWindows())
            {
                WriteDebug("Hiding cursor using Console.CursorVisible");
                Console.CursorVisible = false;
                didHideCursor = true;
            }

            try
            {
                var windowHeight = Host.UI.RawUI.WindowSize.Height;
                var calculatedHeight = Height?.Value?.CalculateAbsoluteValue(windowHeight);

                var mainWindow = new MainWindow(
                    Host.UI,
                    KeyBindings ?? KeyBindings.Empty,
                    inputObjects,
                    Host.UI.RawUI.WindowSize.Width,
                    calculatedHeight.GetValueOrDefault(20),
                    Vertical.IsPresent ? SplitDirection.Vertical : SplitDirection.Horizontal,
                    Preview);

                var result = mainWindow.RunMainLoop(Host.UI);
                WriteObject(result.SelectedItems, enumerateCollection: true);
            }
            finally
            {
                if (didHideCursor)
                {
                    if (Host.UI.SupportsVirtualTerminal)
                        Host.UI.Write(EscapeSequence.ShowCursor);
                    else if (OperatingSystem.IsWindows())
                        Console.CursorVisible = true;
                }
            }
        }
        else
        {
            WriteDebug("Item array is empty");
        }
    }

    private readonly List<InputObject> pipedObjects = new();

    private IReadOnlyList<InputObject> CreateInputObjectCollection(IReadOnlyList<PSObject?> inputItems)
    {
        var result = new List<InputObject>(inputItems.Count);
        for (int i = 0; i < inputItems.Count; i++)
            result.Add(CreateInputObject(inputItems[i], i));
        return result;
    }

    private InputObject CreateInputObject(PSObject? inputObject, int itemIndex)
    {
        var rawText = GetItemText(inputObject, itemIndex);
        var parsedText = ConsoleString.CreatePlainText(rawText);
        return new InputObject(parsedText, inputObject);
    }

    private string GetItemText(PSObject? item, int itemIndex)
    {
        string GetDefaultText()
            => item?.ToString() ?? $"(null #{itemIndex})";

        if (Property is not null)
        {
            var result = Property.GetValues(item);
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
                        new ItemCallbackScriptException(nameof(Property), itemIndex, firstResult.Exception),
                        "SelectInteractive_FormatLabel_RuntimeException",
                        ErrorCategory.InvalidOperation,
                        item));
                }
            }
        }

        return GetDefaultText();
    }
}
