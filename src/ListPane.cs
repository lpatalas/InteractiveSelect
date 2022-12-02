using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;

namespace InteractiveSelect;

internal class ListPane
{
    private readonly ListView<ListItem> listItems;
    private readonly Action<PSObject?> highlightedItemChangedCallback;

    public PSObject? HighlightedObject => listItems.HighlightedItem?.Value;

    public ListPane(
        IReadOnlyList<ListItem> listItems,
        int height,
        Action<PSObject?> highlightedItemChangedCallback)
    {
        var listPageSize = height - 1; // -1 to make space for filter line
        this.listItems = new ListView<ListItem>(
            listItems,
            listPageSize,
            (item, filter) => item.Label.Contains(filter, StringComparison.OrdinalIgnoreCase),
            listItem => highlightedItemChangedCallback(listItem?.Value));
        this.highlightedItemChangedCallback = highlightedItemChangedCallback;
    }

    public IEnumerable<PSObject?> GetSelectedObjects()
    {
        var highlightedItem = listItems.HighlightedItem;
        if (highlightedItem is not null)
            return new[] { highlightedItem.Value };
        else
            return Enumerable.Empty<PSObject?>();
    }

    public bool HandleKey(ConsoleKeyInfo keyInfo)
    {
        switch (keyInfo.Key)
        {
            case ConsoleKey.Escape:
                if (!string.IsNullOrEmpty(listItems.Filter))
                {
                    listItems.Filter = string.Empty;
                    return true;
                }
                break;
            case ConsoleKey.UpArrow:
                listItems.SetHighlightedIndex(listItems.HighlightedIndex - 1);
                return true;
            case ConsoleKey.DownArrow:
                listItems.SetHighlightedIndex(listItems.HighlightedIndex + 1);
                return true;
            case ConsoleKey.Home:
                listItems.SetHighlightedIndex(0);
                return true;
            case ConsoleKey.End:
                listItems.SetHighlightedIndex(listItems.Count - 1);
                return true;
            case ConsoleKey.PageUp:
                listItems.SetHighlightedIndex(listItems.HighlightedIndex - listItems.PageSize + 1);
                return true;
            case ConsoleKey.PageDown:
                listItems.SetHighlightedIndex(listItems.HighlightedIndex + listItems.PageSize - 1);
                return true;
            case ConsoleKey.Backspace:
                if (!string.IsNullOrEmpty(listItems.Filter))
                    listItems.Filter = listItems.Filter.Substring(0, listItems.Filter.Length - 1);
                return true;
            default:
                if (char.IsLetterOrDigit(keyInfo.KeyChar) || char.IsPunctuation(keyInfo.KeyChar))
                {
                    listItems.Filter = (listItems.Filter ?? "") + keyInfo.KeyChar;
                    return true;
                }
                break;
        }

        return false;
    }

    public void Draw(Canvas canvas)
    {
        DrawFilter(canvas);
        DrawItems(canvas);
    }

    private void DrawFilter(Canvas canvas)
    {
        var filterText = listItems.Filter switch
        {
            null or "" => $"{PSStyle.Instance.Foreground.BrightBlack}(no filter)",
            _ => $"{PSStyle.Instance.Foreground.BrightBlue}> {listItems.Filter}"
        };

        canvas.FillLine(0, ConsoleString.CreateStyled(filterText));
    }

    private void DrawItems(Canvas canvas)
    {
        for (int lineIndex = 0; lineIndex < listItems.PageSize; lineIndex++)
        {
            int itemIndex = lineIndex + listItems.ScrollOffset;
            var backgroundColor = (listItems.HighlightedIndex == itemIndex) switch
            {
                true => ConsoleString.CreateStyled(PSStyle.Instance.Background.Red),
                false => ConsoleString.Empty
            };

            var text = itemIndex < listItems.Count
                ? listItems[itemIndex].Label
                : ConsoleString.Empty;

            canvas.FillLine(lineIndex + 1, ConsoleString.Concat(backgroundColor, text));
        }
    }
}
