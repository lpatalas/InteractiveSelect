using System.Management.Automation;
using System.Management.Automation.Host;
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

    public void FillLine(int lineIndex, ConsoleString text)
    {
        if (lineIndex >= area.GetHeight())
            throw new PSArgumentOutOfRangeException(nameof(lineIndex));

        var lineWidth = area.GetWidth();
        if (text.ContentLength > lineWidth)
            text = text.AddEllipsis(lineWidth);

        buffer.Clear();
        buffer.Append(text);
        if (text.ContentLength < lineWidth)
            buffer.Append(' ', lineWidth - text.ContentLength);
        buffer.Append(PSStyle.Instance.Reset);

        hostUI.RawUI.CursorPosition = new Coordinates(area.Left, area.Top + lineIndex);
        hostUI.Write(buffer.ToString());
    }

    public void Clear()
    {
        for (var y = 0; y < area.GetHeight(); y++)
            FillLine(y, ConsoleString.Empty);

        hostUI.RawUI.CursorPosition = area.GetTopLeft();
    }
}
