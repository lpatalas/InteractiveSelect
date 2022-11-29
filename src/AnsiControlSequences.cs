namespace InteractiveSelect;

internal static class AnsiControlSequences
{
    public const string ShowCursor = $"\x1b[?25h";
    public const string HideCursor = $"\x1b[?25l";
}
