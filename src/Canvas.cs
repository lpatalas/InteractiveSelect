using System.Management.Automation;
using System.Management.Automation.Host;
using System.Text;

namespace InteractiveSelect;

internal class Canvas
{
    private readonly Rect area;
    private readonly StringBuilder buffer;
    private readonly PSHostUserInterface hostUI;

    public int Width => area.Width;
    public int Height => area.Height;

    public Canvas(PSHostUserInterface hostUI, Rect area)
    {
        this.area = area;
        this.hostUI = hostUI;

        // capacity is line width plus extra space for ANSI control sequences
        buffer = new StringBuilder(capacity: area.Width * 2);
    }

    public Canvas GetSubArea(int x, int y, int width, int height)
    {
        var subArea = new Rect(area.X + x, area.Y + y, width, height);
        return new Canvas(hostUI, subArea);
    }

    public void DrawHeader(bool isActive, ConsoleString text)
    {
        string? style = isActive switch
        {
            true => Theme.Instance.HeaderActive,
            false => Theme.Instance.HeaderInactive,
        };

        var borderStyle = Theme.Instance.Border;

        // initial dash and two spaces around caption
        var paddingLength = Width - text.ContentLength - 4;

        if (paddingLength >= 0)
        {
            var styledText = ConsoleString.Concat(
                ConsoleString.CreateStyled(style + "[ " + text + " ]"),
                ConsoleString.CreateStyled(borderStyle + new string('\u2500', paddingLength)));
            FillLine(0, styledText);
        }
        else
        {
            FillLine(0, ConsoleString.CreateStyled(style + new string('\u2500', Width)));
        }

    }

    public void FillLine(int lineIndex, ConsoleString text)
    {
        if (lineIndex >= area.Height)
            throw new PSArgumentOutOfRangeException(nameof(lineIndex));

        var lineWidth = area.Width;
        if (text.ContentLength > lineWidth)
            text = text.AddEllipsis(lineWidth);

        buffer.Clear();
        buffer.Append(text);
        if (text.ContentLength < lineWidth)
            buffer.Append(' ', lineWidth - text.ContentLength);
        buffer.Append(EscapeSequence.Reset.ToString());

        hostUI.RawUI.CursorPosition = new Coordinates(area.X, area.Y + lineIndex);
        hostUI.Write(buffer.ToString());
    }

    public void Clear()
    {
        for (var y = 0; y < area.Height; y++)
            FillLine(y, ConsoleString.Empty);

        hostUI.RawUI.CursorPosition = area.TopLeft;
    }
}
