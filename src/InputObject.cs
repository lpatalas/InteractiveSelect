using System.Management.Automation;

namespace InteractiveSelect;

internal class InputObject
{
    public bool IsSelected { get; set; }
    public ConsoleString Label { get; }
    public PSObject? Value { get; }

    public InputObject(ConsoleString label, PSObject? value)
    {
        Label = label;
        Value = value;
    }

    public void ToggleSelection()
        => IsSelected = !IsSelected;
}
