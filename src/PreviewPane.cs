using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveSelect;

internal class PreviewPane
{
    private static readonly IReadOnlyList<string> colors = new[]
    {
        PSStyle.Instance.Background.Cyan,
        PSStyle.Instance.Background.Magenta,
        PSStyle.Instance.Background.Yellow,
        PSStyle.Instance.Background.Green,
    };

    private int currentColor = 0;

    public bool HandleKey(ConsoleKeyInfo keyInfo)
    {
        return false;
    }

    public void Draw(Canvas canvas)
    {
        for (int y = 0; y < canvas.Height; y++)
        {
            canvas.FillLine(y, colors[currentColor]);
            currentColor = (currentColor + 1) % colors.Count;
        }
    }
}
