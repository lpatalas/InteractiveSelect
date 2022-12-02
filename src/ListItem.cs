using System.Management.Automation;

namespace InteractiveSelect;

internal record ListItem(ConsoleString Label, PSObject? Value)
{
}
