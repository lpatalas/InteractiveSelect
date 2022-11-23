using System;
using System.Management.Automation.Host;
using System.Text;

namespace InteractiveSelect;

internal class LineRenderer
{
    private readonly StringBuilder buffer;
    private readonly PSHostUserInterface hostUI;
    private readonly int lineWidth;

    public LineRenderer(PSHostUserInterface hostUI, int lineWidth)
    {
        this.hostUI = hostUI;
        this.lineWidth = lineWidth;

        buffer = new StringBuilder(capacity: lineWidth);
    }

    public void DrawLine(
        string text,
        Coordinates coordinates,
        ConsoleColor foregroundColor,
        ConsoleColor backgroundColor)
    {
        buffer.Clear();
        buffer.Append(text);

        if (buffer.Length < lineWidth)
            buffer.Append(' ', lineWidth - buffer.Length);

        hostUI.RawUI.CursorPosition = coordinates;
        hostUI.Write(foregroundColor, backgroundColor, buffer.ToString());
    }
}
