using System;
using System.Collections.Generic;
using System.Management.Automation.Host;
using System.Management.Automation.Language;
using System.Text;

namespace InteractiveSelect;

internal class ListView
{
    private readonly CollectionView<ListItem> listItems;

    public ListView(IReadOnlyList<ListItem> listItems, int pageSize)
    {
        this.listItems = new CollectionView<ListItem>(
            listItems,
            pageSize,
            (item, filter) => item.Label.Contains(filter, StringComparison.OrdinalIgnoreCase));
    }

    public void RunLoop(PSHostUserInterface hostUI)
    {
        var actualHeight = Math.Min(listItems.Count, listItems.PageSize + 1);
        var area = new Rectangle(
            0,
            hostUI.RawUI.CursorPosition.Y,
            hostUI.RawUI.BufferSize.Width,
            hostUI.RawUI.CursorPosition.Y + actualHeight);

        var isExiting = false;
        while (!isExiting)
        {
            Draw(area, hostUI);

            var pressedKey = Console.ReadKey(intercept: true);
            switch (pressedKey.Key)
            {
                case ConsoleKey.Escape:
                    if (listItems.Filter.Length > 0)
                        listItems.Filter = string.Empty;
                    else
                        isExiting = true;
                    break;
                case ConsoleKey.UpArrow:
                    listItems.SetHighlightedIndex(listItems.HighlightedIndex - 1);
                    break;
                case ConsoleKey.DownArrow:
                    listItems.SetHighlightedIndex(listItems.HighlightedIndex + 1);
                    break;
                case ConsoleKey.Home:
                    listItems.SetHighlightedIndex(0);
                    break;
                case ConsoleKey.End:
                    listItems.SetHighlightedIndex(listItems.Count - 1);
                    break;
                case ConsoleKey.PageUp:
                    listItems.SetHighlightedIndex(listItems.HighlightedIndex - listItems.PageSize + 1);
                    break;
                case ConsoleKey.PageDown:
                    listItems.SetHighlightedIndex(listItems.HighlightedIndex + listItems.PageSize - 1);
                    break;
                case ConsoleKey.Backspace:
                    if (listItems.Filter != null && listItems.Filter.Length > 0)
                        listItems.Filter = listItems.Filter.Substring(0, listItems.Filter.Length - 1);
                    break;
                default:
                    if (char.IsLetter(pressedKey.KeyChar))
                    {
                        listItems.Filter = (listItems.Filter ?? "") + pressedKey.KeyChar;
                    }
                    break;
            }
        }

        ClearConsole(area, hostUI);
    }

    private void Draw(Rectangle area, PSHostUserInterface hostUI)
    {
        DrawFilter(new Rectangle(area.Left, area.Top, area.Right, area.Top + 1), hostUI);
        DrawItems(new Rectangle(area.Left, area.Top + 1, area.Right, area.Bottom), hostUI);
    }

    private void DrawFilter(Rectangle area, PSHostUserInterface hostUI)
    {
        var lineRenderer = new LineRenderer(hostUI, area.GetWidth());

        if (listItems.Filter.Length > 0)
        {
            lineRenderer.DrawLine(
                $"> {listItems.Filter}",
                area.GetTopLeft(),
                ConsoleColor.Yellow,
                ConsoleColor.Black);
        }
        else
        {
            lineRenderer.DrawLine(
                $"(no filter)",
                area.GetTopLeft(),
                ConsoleColor.DarkGray,
                ConsoleColor.Black);
        }
    }

    private void DrawItems(Rectangle area, PSHostUserInterface hostUI)
    {
        var lineWidth = area.GetWidth();
        var lineRenderer = new LineRenderer(hostUI, lineWidth);

        for (int lineIndex = 0; lineIndex < listItems.PageSize; lineIndex++)
        {
            int itemIndex = lineIndex + listItems.ScrollOffset;
            var backgroundColor = (listItems.HighlightedIndex == itemIndex) switch
            {
                true => ConsoleColor.Red,
                false => ConsoleColor.DarkGray
            };

            var pos = new Coordinates(area.Left, area.Top + lineIndex);
            var text = itemIndex < listItems.Count ? listItems[itemIndex].Label : string.Empty;
            lineRenderer.DrawLine(text, pos, ConsoleColor.Cyan, backgroundColor);
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
