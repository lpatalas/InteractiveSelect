using System.Management.Automation;

namespace InteractiveSelect;

internal class ListItem
{
    public bool IsSelected { get; set; }
    public ConsoleString Label { get; }
    public PSObject? Value { get; }

    public ListItem(ConsoleString label, PSObject? value)
    {
        Label = label;
        Value = value;
    }

    public void ToggleSelection()
        => IsSelected = !IsSelected;
}
