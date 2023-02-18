using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;

namespace InteractiveSelect;

internal class ListPane
{
    private const int headerHeight = 1;
    private const int scrollBarWidth = 1;

    private readonly ListView<InputObject> listView;
    private readonly Action<PSObject?> highlightedItemChangedCallback;

    public PSObject? HighlightedObject => listView.HighlightedItemValue?.Value;
    public int Width { get; }

    public ListPane(
        IReadOnlyList<InputObject> inputObjects,
        int maximumWidth,
        int height,
        Action<PSObject?> highlightedItemChangedCallback)
    {
        var listPageSize = height - headerHeight;
        this.listView = new ListView<InputObject>(
            inputObjects,
            listPageSize,
            (item, filter) => item.Label.Contains(filter, StringComparison.OrdinalIgnoreCase),
            listItem => highlightedItemChangedCallback(listItem?.Value));
        this.highlightedItemChangedCallback = highlightedItemChangedCallback;

        int maxItemWidth = inputObjects.Max(x => x.Label.ContentLength);
        Width = Math.Min(maxItemWidth + scrollBarWidth, maximumWidth);
    }

    public IEnumerable<PSObject?> GetSelectedObjects()
    {
        var selectedItems = listView.Items
            .Where(x => x.IsSelected)
            .Select(x => x.Value)
            .ToList();

        if (selectedItems.Count > 0)
            return selectedItems;
        else if (listView.HighlightedItemValue is InputObject highlightedItem)
            return new[] { highlightedItem.Value };
        else
            return Enumerable.Empty<PSObject?>();
    }

    public bool HandleKey(ConsoleKeyInfo keyInfo)
    {
        switch (keyInfo.Key)
        {
            case ConsoleKey.Escape:
                if (!string.IsNullOrEmpty(listView.Filter))
                {
                    listView.Filter = string.Empty;
                    return true;
                }
                break;
            case ConsoleKey.UpArrow:
                listView.SetHighlightedIndex(listView.HighlightedIndex - 1);
                return true;
            case ConsoleKey.DownArrow:
                listView.SetHighlightedIndex(listView.HighlightedIndex + 1);
                return true;
            case ConsoleKey.Home:
                listView.SetHighlightedIndex(0);
                return true;
            case ConsoleKey.End:
                listView.SetHighlightedIndex(listView.Count - 1);
                return true;
            case ConsoleKey.PageUp:
                listView.SetHighlightedIndex(listView.HighlightedIndex - listView.PageSize + 1);
                return true;
            case ConsoleKey.PageDown:
                listView.SetHighlightedIndex(listView.HighlightedIndex + listView.PageSize - 1);
                return true;
            case ConsoleKey.Backspace:
                if (!string.IsNullOrEmpty(listView.Filter))
                    listView.Filter = listView.Filter.Substring(0, listView.Filter.Length - 1);
                return true;
            case ConsoleKey.Spacebar:
                if (listView.HighlightedItemValue is InputObject highlightedItem)
                {
                    highlightedItem.ToggleSelection();
                    return true;
                }
                return false;
            default:
                if (char.IsLetterOrDigit(keyInfo.KeyChar) || char.IsPunctuation(keyInfo.KeyChar))
                {
                    listView.Filter = (listView.Filter ?? "") + keyInfo.KeyChar;
                    return true;
                }
                break;
        }

        return false;
    }

    public void Draw(Canvas canvas, bool isActive)
    {
        DrawFilter(canvas, isActive);

        var listViewCanvas = canvas.GetSubArea(0, 1, canvas.Width, canvas.Height - 1);
        listView.Draw(listViewCanvas, getLabel: item => item.Label);
    }

    private void DrawFilter(Canvas canvas, bool isActive)
    {
        var totalCount = listView.OriginalItems.Count;
        var filteredCount = listView.Count;

        var filterText = listView.Filter switch
        {
            null or "" => $"{filteredCount}/{totalCount}",
            _ => $"{filteredCount}/{totalCount}> {listView.Filter}"
        };

        canvas.DrawHeader(isActive, ConsoleString.CreatePlainText(filterText));
    }
}
