﻿using System;
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
        var listPageSize = height - 1; // -1 to make space for header
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

    public void Draw(Canvas canvas, bool isActive)
    {
        DrawFilter(canvas, isActive);
        DrawList(canvas.GetSubArea(0, 1, canvas.Width, canvas.Height - 1));
    }

    private void DrawFilter(Canvas canvas, bool isActive)
    {
        var totalCount = listItems.OriginalItems.Count;
        var filteredCount = listItems.Count;

        var filterText = listItems.Filter switch
        {
            null or "" => $"{filteredCount}/{totalCount}",
            _ => $"{filteredCount}/{totalCount}> {listItems.Filter}"
        };

        canvas.DrawHeader(isActive, ConsoleString.CreatePlainText(filterText));
    }

    private void DrawList(Canvas canvas)
    {
        var itemsCanvas = canvas.GetSubArea(0, 0, canvas.Width - 1, canvas.Height);
        var scrollBarCanvas = canvas.GetSubArea(canvas.Width - 1, 0, 1, canvas.Height);

        DrawItems(itemsCanvas);
        DrawScrollBar(scrollBarCanvas);
    }

    private void DrawItems(Canvas canvas)
    {
        for (int lineIndex = 0; lineIndex < listItems.PageSize; lineIndex++)
        {
            int itemIndex = lineIndex + listItems.ScrollOffset;
            var backgroundColor = (listItems.HighlightedIndex == itemIndex) switch
            {
                true => ConsoleString.CreateStyled(Theme.Instance.ItemHighlighted),
                false => ConsoleString.CreateStyled(Theme.Instance.ItemNormal)
            };

            var text = itemIndex < listItems.Count
                ? listItems[itemIndex].Label
                : ConsoleString.Empty;

            var line = ConsoleString.Concat(backgroundColor, text);
            canvas.FillLine(lineIndex, line);
        }
    }

    private void DrawScrollBar(Canvas canvas)
    {
        var scrollBar = ScrollBarLayout.Compute(
            canvas.Height,
            listItems.ScrollOffset,
            listItems.PageSize,
            listItems.Count);

        for (int i = 0; i < canvas.Height; i++)
        {
            var glyph = scrollBar.GetVerticalGlyph(i);
            canvas.FillLine(i, ConsoleString.CreatePlainText(glyph.ToString()));
        }
    }
}
