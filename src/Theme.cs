using System;

namespace InteractiveSelect;

internal class Theme
{
    public static Theme Instance { get; } = new Theme();

    public string Border { get; } = EscapeSequence.MakeForegroundColor(ConsoleColor.DarkGray);
    public string HeaderActive { get; set; } = EscapeSequence.MakeForegroundColor(ConsoleColor.White);
    public string HeaderInactive { get; set; } = EscapeSequence.MakeForegroundColor(ConsoleColor.DarkGray);
    public string ItemNormal { get; set; } = string.Empty;
    public string ItemHighlighted { get; set; } = EscapeSequence.MakeBackgroundColor(ConsoleColor.DarkRed);
}
