using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace InteractiveSelect;

internal static class ScriptBlockExtensions
{
    public static Collection<PSObject> InvokeWithInputObject(
        this ScriptBlock scriptBlock,
        object? inputObject)
    {
        var variables = new List<PSVariable>(1)
        {
            new PSVariable("_", inputObject)
        };

        var results = scriptBlock.InvokeWithContext(
            functionsToDefine: null,
            variablesToDefine: variables);

        return results;
    }
}
