using System;

namespace InteractiveSelect;

internal static class ConsoleColorExtensions
{
    public static int ToForegroundColorCode(this ConsoleColor color)
        => color switch
        {
            ConsoleColor.Black => 30,
            ConsoleColor.Blue => 94,
            ConsoleColor.Cyan => 96,
            ConsoleColor.DarkBlue => 34,
            ConsoleColor.DarkCyan => 36,
            ConsoleColor.DarkGray => 90,
            ConsoleColor.DarkGreen => 32,
            ConsoleColor.DarkMagenta => 35,
            ConsoleColor.DarkRed => 31,
            ConsoleColor.DarkYellow => 33,
            ConsoleColor.Gray => 37,
            ConsoleColor.Green => 92,
            ConsoleColor.Magenta => 95,
            ConsoleColor.Red => 91,
            ConsoleColor.White => 97,
            ConsoleColor.Yellow => 93,
            _ => 37,
        };

    public static int ToBackgroundColorCode(this ConsoleColor color)
        => ToForegroundColorCode(color) + 10;
}
