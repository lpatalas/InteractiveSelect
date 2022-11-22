using System.Management.Automation;

namespace InteractiveSelect;

internal record ListItem(string Label, PSObject? Value)
{
}
