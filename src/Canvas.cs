using System;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Internal;
using System.Text;

namespace InteractiveSelect;

internal class Canvas
{
    private readonly Rectangle area;
    private readonly StringBuilder buffer;
    private readonly PSHostUserInterface hostUI;

    public int Width => area.GetWidth();
    public int Height => area.GetHeight();

    public Canvas(PSHostUserInterface hostUI, Rectangle area)
    {
        this.area = area;
        this.hostUI = hostUI;

        // capacity is line width plus extra space for ANSI control sequences
        buffer = new StringBuilder(capacity: area.GetWidth() * 2);
    }

    public void FillLine(int lineIndex, string text)
    {
        if (lineIndex >= area.GetHeight())
            throw new PSArgumentOutOfRangeException(nameof(lineIndex));

        var lineWidth = area.GetWidth();
        var textDecorated = new ValueStringDecorated(text);
        if (textDecorated.ContentLength > lineWidth)
            textDecorated = textDecorated.AddEllipsis(lineWidth);

        buffer.Clear();
        buffer.Append(textDecorated);
        if (textDecorated.ContentLength < lineWidth)
            buffer.Append(' ', lineWidth - textDecorated.ContentLength);
        buffer.Append(PSStyle.Instance.Reset);

        hostUI.RawUI.CursorPosition = new Coordinates(area.Left, area.Top + lineIndex);
        hostUI.Write(buffer.ToString());
    }

    public void Clear()
    {
        for (var y = 0; y < area.GetHeight(); y++)
            FillLine(y, string.Empty);

        hostUI.RawUI.CursorPosition = area.GetTopLeft();
    }
}
