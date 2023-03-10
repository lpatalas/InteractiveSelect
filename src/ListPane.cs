using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;

namespace InteractiveSelect;

internal class ListPane
{
    private const int headerHeight = 1;
    private readonly ListView<InputObject> listView;
    private readonly int maxItemWidth;
    private const int scrollBarWidth = 1;

    public PSObject? HighlightedValue => listView.HighlightedValue?.Value;

    public int Width { get; private set; }
    public int Height { get; private set; }

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

        maxItemWidth = inputObjects.Max(x => x.Label.ContentLength);

        Width = Math.Min(maxItemWidth + scrollBarWidth, maximumWidth);
        Height = height;
    }

    public Size GetMaximumSize()
        => new Size(maxItemWidth, listView.TotalItemCount);

    public void Resize(int newWidth, int newHeight)
    {
        Width = newWidth;
        Height = newHeight;
        listView.SetPageSize(Height - headerHeight);
    }

    public IEnumerable<PSObject?> GetSelectedObjects()
    {
        return listView.GetSelectedItems().Select(item => item.Value);
    }

    public bool HandleKey(ConsoleKeyInfo keyInfo)
    {
        bool ShouldToggleSelection()
            => keyInfo.Modifiers.HasFlag(ConsoleModifiers.Shift);

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
                listView.HighlightPreviousItem(toggleSelection: ShouldToggleSelection());
                return true;
            case ConsoleKey.DownArrow:
                listView.HighlightNextItem(toggleSelection: ShouldToggleSelection());
                return true;
            case ConsoleKey.Home:
                listView.HighlightFirstItem(toggleSelection: ShouldToggleSelection());
                return true;
            case ConsoleKey.End:
                listView.HighlightLastItem(toggleSelection: ShouldToggleSelection());
                return true;
            case ConsoleKey.PageUp:
                listView.HighlightItemPageUp(toggleSelection: ShouldToggleSelection());
                return true;
            case ConsoleKey.PageDown:
                listView.HighlightItemPageDown(toggleSelection: ShouldToggleSelection());
                return true;
            case ConsoleKey.Backspace:
                if (!string.IsNullOrEmpty(listView.Filter))
                    listView.Filter = listView.Filter.Substring(0, listView.Filter.Length - 1);
                return true;
            case ConsoleKey.Spacebar:
                listView.ToggleSelection();
                return true;
            case ConsoleKey.A:
                if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control))
                {
                    listView.SelectAll();
                    return true;
                }
                break;
            case ConsoleKey.D:
                if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control))
                {
                    listView.UnselectAll();
                    return true;
                }
                break;
            case ConsoleKey.I:
                if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control))
                {
                    listView.InvertSelection();
                    return true;
                }
                break;
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
        var totalCount = listView.TotalItemCount;
        var filteredCount = listView.VisibleItemCount;

        var filterText = listView.Filter switch
        {
            null or "" => $"{filteredCount}/{totalCount}",
            _ => $"{filteredCount}/{totalCount}> {listView.Filter}"
        };

        canvas.DrawHeader(isActive, ConsoleString.CreatePlainText(filterText));
    }
}
