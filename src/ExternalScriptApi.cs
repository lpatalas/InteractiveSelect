namespace InteractiveSelect;

public interface IExternalScriptApi
{
    object? HighlightedValue { get; }

    void Exit();
    void Exit(object result);
}

internal class ExternalScriptApi : IExternalScriptApi
{
    internal object? Result { get; private set; }
    internal bool WasExitRequested { get; private set; }

    public object? HighlightedValue { get; }

    public ExternalScriptApi(object? highlightedValue)
    {
        HighlightedValue = highlightedValue;
    }

    public void Exit()
    {
        WasExitRequested = true;
    }

    public void Exit(object result)
    {
        Result = result;
        WasExitRequested = true;
    }
}
