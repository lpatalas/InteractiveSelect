using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Internal;
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

    public IEnumerable<PSObject?> SelectItems(PSHostUserInterface hostUI)
    {
        var actualHeight = Math.Min(listItems.Count, listItems.PageSize + 1);
        var area = new Rectangle(
            0,
            hostUI.RawUI.CursorPosition.Y,
            hostUI.RawUI.BufferSize.Width,
            hostUI.RawUI.CursorPosition.Y + actualHeight);

        var result = Enumerable.Empty<PSObject?>();
        var isExiting = false;
        while (!isExiting)
        {
            Draw(area, hostUI);

            var pressedKey = Console.ReadKey(intercept: true);
            switch (pressedKey.Key)
            {
                case ConsoleKey.Enter:
                    if (listItems.HighlightedItem is ListItem item)
                    {
                        isExiting = true;
                        result = new[] { item.Value };
                    }
                    break;
                case ConsoleKey.Escape:
                    if (!string.IsNullOrEmpty(listItems.Filter))
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
                    if (!string.IsNullOrEmpty(listItems.Filter))
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
        return result;
    }

    private void Draw(Rectangle area, PSHostUserInterface hostUI)
    {
        DrawFilter(new Rectangle(area.Left, area.Top, area.Right, area.Top + 1), hostUI);
        DrawItems(new Rectangle(area.Left, area.Top + 1, area.Right, area.Bottom), hostUI);
    }

    private void DrawFilter(Rectangle area, PSHostUserInterface hostUI)
    {
        var lineRenderer = new LineRenderer(hostUI, area.GetWidth());

        if (!string.IsNullOrEmpty(listItems.Filter))
        {
            lineRenderer.DrawLine(
                new StringDecorated($"{PSStyle.Instance.Foreground.BrightBlue}> {listItems.Filter}"),
                area.GetTopLeft());
        }
        else
        {
            lineRenderer.DrawLine(
                new StringDecorated($"{PSStyle.Instance.Foreground.BrightBlack}(no filter)"),
                area.GetTopLeft());
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
                true => PSStyle.Instance.Background.Red,
                false => string.Empty
            };

            var pos = new Coordinates(area.Left, area.Top + lineIndex);
            var text = itemIndex < listItems.Count ? listItems[itemIndex].Label : string.Empty;
            lineRenderer.DrawLine(new StringDecorated($"{backgroundColor}{text}"), pos);
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
