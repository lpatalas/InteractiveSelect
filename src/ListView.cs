using System;
using System.Collections.Generic;
using System.Management.Automation.Host;
using System.Management.Automation.Language;
using System.Text;

namespace InteractiveSelect;

internal class ListView
{
    private int highlightedIndex;
    private readonly IReadOnlyList<ListItem> listItems;
    private int scrollOffset;
    private readonly int pageSize;

    public ListView(IReadOnlyList<ListItem> listItems, int pageSize)
    {
        this.listItems = listItems;
        this.pageSize = pageSize;
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
                    SetHighlightedIndex(highlightedIndex - 1);
                    break;
                case ConsoleKey.DownArrow:
                    SetHighlightedIndex(highlightedIndex + 1);
                    break;
                case ConsoleKey.Home:
                    SetHighlightedIndex(0);
                    break;
                case ConsoleKey.End:
                    SetHighlightedIndex(listItems.Count - 1);
                    break;
                case ConsoleKey.PageUp:
                    SetHighlightedIndex(highlightedIndex - pageSize + 1);
                    break;
                case ConsoleKey.PageDown:
                    SetHighlightedIndex(highlightedIndex + pageSize - 1);
                    break;
            }
        }

        ClearConsole(area, hostUI);
    }

    private void SetHighlightedIndex(int newIndex)
    {
        highlightedIndex = Math.Clamp(newIndex, 0, listItems.Count - 1);
        if (highlightedIndex < scrollOffset)
            scrollOffset = highlightedIndex;
        else if (highlightedIndex >= scrollOffset + pageSize)
            scrollOffset = highlightedIndex - pageSize + 1;
    }

    private void DrawItems(Rectangle area, PSHostUserInterface hostUI)
    {
        var lineWidth = area.GetWidth();
        var lineBuffer = new StringBuilder(lineWidth);

        for (int lineIndex = 0; lineIndex < pageSize; lineIndex++)
        {
            lineBuffer.Clear();

            int itemIndex = lineIndex + scrollOffset;
            var backgroundColor = (highlightedIndex == itemIndex) switch
            {
                true => ConsoleColor.Red,
                false => ConsoleColor.DarkGray
            };

            var item = listItems[itemIndex];
            hostUI.RawUI.CursorPosition = new Coordinates(area.Left, area.Top + lineIndex);
            lineBuffer.Append(item.Label);
            if (lineBuffer.Length < lineWidth)
                lineBuffer.Append(' ', lineWidth - lineBuffer.Length);

            hostUI.Write(ConsoleColor.Cyan, backgroundColor, lineBuffer.ToString());
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
