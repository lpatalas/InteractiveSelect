using System;
using System.Collections.Generic;
using System.Management.Automation.Host;
using System.Management.Automation.Language;

namespace InteractiveSelect;

internal class ListView
{
    private readonly IReadOnlyList<ListItem> listItems;
    private int highlightedIndex;

    public ListView(IReadOnlyList<ListItem> listItems)
    {
        this.listItems = listItems;
    }

    public void RunLoop(PSHostUserInterface hostUI, int maxHeight)
    {
        var actualHeight = Math.Min(listItems.Count, maxHeight);
        var area = new Rectangle(
            0,
            hostUI.RawUI.CursorPosition.Y,
            hostUI.RawUI.BufferSize.Width,
            hostUI.RawUI.CursorPosition.Y + actualHeight);

        var isExiting = false;
        while (!isExiting)
        {
            DrawItems(area, hostUI);
            var pressedKey = Console.ReadKey(intercept: true);
            switch (pressedKey.Key)
            {
                case ConsoleKey.Escape:
                    isExiting = true;
                    break;
                case ConsoleKey.UpArrow:
                    highlightedIndex = Math.Max(0, highlightedIndex - 1);
                    break;
                case ConsoleKey.DownArrow:
                    highlightedIndex = Math.Min(listItems.Count - 1, highlightedIndex + 1);
                    break;
            }
        }

        ClearConsole(area, hostUI);
    }

    private void DrawItems(Rectangle area, PSHostUserInterface hostUI)
    {
        for (int i = 0; i < area.GetHeight(); i++)
        {
            var backgroundColor = (highlightedIndex == i) switch
            {
                true => ConsoleColor.Red,
                false => ConsoleColor.DarkGray
            };

            var item = listItems[i];
            hostUI.RawUI.CursorPosition = new Coordinates(area.Left, area.Top + i);
            hostUI.Write(ConsoleColor.Cyan, backgroundColor, item.Label);
        }
    }

    private void ClearConsole(Rectangle rectangle, PSHostUserInterface hostUI)
    {
        var lineWidth = rectangle.GetWidth();
        var blankString = new string(' ', lineWidth);
        for (var y = rectangle.Top; y < rectangle.Bottom; y++)
        {
            hostUI.RawUI.CursorPosition = new Coordinates(rectangle.Left, y);
            hostUI.Write(blankString);
        }

        hostUI.RawUI.CursorPosition = rectangle.GetTopLeft();
    }
}
