using System;

namespace InteractiveSelect;

internal class ItemCallbackScriptException : Exception
{
    public ItemCallbackScriptException(
        string callbackName,
        int itemIndex,
        Exception innerException)
        : base(
            $"Error when calling {callbackName} script for item {itemIndex}: {innerException.Message}",
            innerException)
    {
    }
}
