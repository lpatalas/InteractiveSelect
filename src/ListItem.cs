using System.Management.Automation;

namespace InteractiveSelect;

internal readonly record struct ListItem(string Label, PSObject? Value)
{
}
