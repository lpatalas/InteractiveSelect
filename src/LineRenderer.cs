using System;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Internal;
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

        buffer = new StringBuilder(capacity: lineWidth * 2);
    }

    public void DrawLine(
        StringDecorated text,
        Coordinates coordinates)
    {
        buffer.Clear();
        buffer.Append(text);

        if (text.ContentLength < lineWidth)
            buffer.Append(' ', lineWidth - text.ContentLength);
        buffer.Append(PSStyle.Instance.Reset);

        hostUI.RawUI.CursorPosition = coordinates;
        hostUI.Write(buffer.ToString());
    }
}
