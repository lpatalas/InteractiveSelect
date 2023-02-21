using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;

namespace InteractiveSelect;

public class KeyBindings
{
    private readonly Dictionary<Keystroke, ScriptBlock> bindings = new Dictionary<Keystroke, ScriptBlock>();

    internal static readonly KeyBindings Empty = new KeyBindings();

    private KeyBindings()
    {
    }

    public KeyBindings(Hashtable input)
    {
        foreach (string key in input.Keys)
        {
            var keystroke = Keystroke.Parse(key);
            var scriptBlock = (ScriptBlock?)input[key];
            if (scriptBlock is null)
                throw new ArgumentException($"Key binding for '{key}' is not a valid script block");

            bindings.Add(keystroke, scriptBlock);
        }
    }

    internal bool HandleKey(ConsoleKeyInfo keyInfo, IExternalScriptApi api)
    {
        foreach (var item in bindings)
        {
            if (item.Key.IsPressed(keyInfo))
            {
                item.Value.Invoke(api);
                return true;
            }
        }

        return false;
    }
}
