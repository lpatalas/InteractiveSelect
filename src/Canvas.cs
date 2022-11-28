using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Internal;
using System.Text;

namespace InteractiveSelect;

internal class Canvas
{
    private static readonly StringDecorated emptyStringDecorated = new StringDecorated(string.Empty);

    private readonly Rectangle area;
    private readonly StringBuilder buffer;
    private readonly PSHostUserInterface hostUI;

    public Canvas(PSHostUserInterface hostUI, Rectangle area)
    {
        this.area = area;
        this.hostUI = hostUI;

        // capacity is line width plus extra space for ANSI control sequences
        buffer = new StringBuilder(capacity: area.GetWidth() * 2);
    }

    public void FillLine(int lineIndex, StringDecorated text)
    {
        if (lineIndex >= area.GetHeight())
            throw new PSArgumentOutOfRangeException(nameof(lineIndex));

        buffer.Clear();
        buffer.Append(text);

        var lineWidth = area.GetWidth();
        if (text.ContentLength < lineWidth)
            buffer.Append(' ', lineWidth - text.ContentLength);
        buffer.Append(PSStyle.Instance.Reset);

        hostUI.RawUI.CursorPosition = new Coordinates(area.Left, area.Top + lineIndex);
        hostUI.Write(buffer.ToString());
    }

    public void Clear()
    {
        for (var y = 0; y < area.GetHeight(); y++)
            FillLine(y, emptyStringDecorated);

        hostUI.RawUI.CursorPosition = area.GetTopLeft();
    }
}
