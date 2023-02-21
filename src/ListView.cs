using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace InteractiveSelect;

internal delegate bool ListFilterPredicate<T>(T item, string filter);

internal class HighlightedItemChangedEventArgs<T> : EventArgs
{
    public T Item { get; }

    public HighlightedItemChangedEventArgs(T item)
    {
        Item = item;
    }
}

file class ListItem<T>
{
    public bool IsSelected { get; set; }
    public T Item { get; }

    public ListItem(T item)
    {
        Item = item;
    }

    public void ToggleSelection()
        => IsSelected = !IsSelected;
}

file class ListItemStructuralEqualityComparer<T> : IEqualityComparer<ListItem<T>>
{
    public static readonly ListItemStructuralEqualityComparer<T> Instance
        = new ListItemStructuralEqualityComparer<T>();

    public bool Equals(ListItem<T>? first, ListItem<T>? second)
    {
        if (first == null && second == null)
            return true;
        if (first == null || second == null)
            return false;

        return first.IsSelected == second.IsSelected
            && Equals(first!.Item, second!.Item);
    }

    public int GetHashCode([DisallowNull] ListItem<T> obj)
        => HashCode.Combine(obj.IsSelected, obj.Item);
}

internal class ListView<T>
    where T : class
{
    private string filter = string.Empty;
    private readonly ListFilterPredicate<T> filterPredicate;
    private int? highlightedIndex;
    private readonly List<ListItem<T>> items;
    private readonly IReadOnlyList<ListItem<T>> originalItems;
    private readonly int pageSize;
    private int scrollOffset;

    private ListItem<T>? HighlightedItem => highlightedIndex.HasValue ? items[highlightedIndex.Value] : default;

    public event EventHandler<HighlightedItemChangedEventArgs<T?>>? HighlightedItemChanged;
    public T? HighlightedValue => HighlightedItem?.Item;

    public int VisibleItemCount => items.Count;
    public int TotalItemCount => originalItems.Count;
    public string Filter { get => filter; set => SetFilter(value); }

    public ListView(
        IReadOnlyList<T> originalItems,
        int pageSize,
        ListFilterPredicate<T> filterPredicate,
        Action<T?>? highlightedItemChangedCallback)
        : this(
              originalItems.Select(x => new ListItem<T>(x)).ToList(),
              scrollOffset: 0,
              pageSize,
              highlightedIndex: null,
              filterPredicate)
    {
        if (highlightedItemChangedCallback is not null)
            HighlightedItemChanged += (sender, e) => highlightedItemChangedCallback(e.Item);
    }

    private ListView(
        IReadOnlyList<ListItem<T>> originalItems,
        int scrollOffset,
        int pageSize,
        int? highlightedIndex,
        ListFilterPredicate<T> filterPredicate)
    {
        if (pageSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        if (scrollOffset < 0 || scrollOffset > Math.Max(0, originalItems.Count - pageSize))
            throw new ArgumentOutOfRangeException(nameof(scrollOffset));
        if (originalItems.Count == 0 && highlightedIndex != null)
            throw new ArgumentException("Highlighted index can't be set when item collection is empty", nameof(highlightedIndex));
        if (highlightedIndex < 0 || highlightedIndex >= originalItems.Count)
            throw new ArgumentOutOfRangeException(nameof(highlightedIndex));

        this.originalItems = originalItems;
        this.items = new List<ListItem<T>>(this.originalItems);
        this.filterPredicate = filterPredicate;

        this.highlightedIndex = highlightedIndex;
        this.pageSize = Math.Min(items.Count, pageSize);
        this.scrollOffset = scrollOffset;
    }

    public IEnumerable<T> GetSelectedItems()
    {
        var selectedItems = items
            .Where(item => item.IsSelected)
            .Select(item => item.Item);

        if (selectedItems.Any())
            return selectedItems;
        else if (HighlightedItem is ListItem<T> highlightedItem)
            return new[] { highlightedItem.Item };
        else
            return Enumerable.Empty<T>();
    }

    public bool HasSameVisibleItems(ListView<T> other)
    {
        return pageSize == other.pageSize
            && scrollOffset == other.scrollOffset
            && highlightedIndex == other.highlightedIndex
            && items.SequenceEqual(other.items, ListItemStructuralEqualityComparer<T>.Instance);
    }

    private void SetFilter(string filter)
    {
        this.filter = filter;

        var oldHighlightedItem = HighlightedItem;

        items.Clear();
        if (filter.Length > 0)
        {
            foreach (var item in originalItems)
            {
                if (filterPredicate(item.Item, filter))
                    items.Add(item);
                else
                    item.IsSelected = false;
            }
        }
        else
        {
            items.AddRange(originalItems);
        }

        if (oldHighlightedItem != null)
        {
            int newHighlightedIndex = items.IndexOf(oldHighlightedItem);
            highlightedIndex = newHighlightedIndex >= 0 ? newHighlightedIndex : null;
        }

        MaintainInvariants();

        if (HighlightedItem != oldHighlightedItem)
        {
            OnHighlightedItemChanged(HighlightedItem?.Item);
        }
    }

    public void HighlightPreviousItem(bool toggleSelection)
        => SetHighlightedIndex(highlightedIndex - 1, toggleSelection);

    public void HighlightNextItem(bool toggleSelection)
        => SetHighlightedIndex(highlightedIndex + 1, toggleSelection);

    public void HighlightItemPageUp(bool toggleSelection)
        => SetHighlightedIndex(highlightedIndex - pageSize + 1, toggleSelection);

    public void HighlightItemPageDown(bool toggleSelection)
        => SetHighlightedIndex(highlightedIndex + pageSize - 1, toggleSelection);

    public void HighlightFirstItem(bool toggleSelection)
        => SetHighlightedIndex(items.Count > 0 ? 0 : null, toggleSelection);

    public void HighlightLastItem(bool toggleSelection)
        => SetHighlightedIndex(items.Count > 0 ? items.Count - 1 : null, toggleSelection);

    public void ToggleSelection()
        => HighlightedItem?.ToggleSelection();

    public void SelectAll()
    {
        foreach (var item in items)
            item.IsSelected = true;
    }

    public void UnselectAll()
    {
        foreach (var item in items)
            item.IsSelected = false;
    }

    public void InvertSelection()
    {
        foreach (var item in items)
            item.ToggleSelection();
    }

    private void SetHighlightedIndex(int? newIndex, bool toggleSelection = false)
    {
        var previousHighlightedIndex = highlightedIndex;
        var previousHighlightedItem = HighlightedItem;

        if (items.Count > 0 && newIndex.HasValue)
            highlightedIndex = Math.Clamp(newIndex.Value, 0, items.Count - 1);
        else
            highlightedIndex = null;

        if (toggleSelection && previousHighlightedIndex.HasValue && highlightedIndex.HasValue)
        {
            if (previousHighlightedIndex == highlightedIndex)
            {
                items[highlightedIndex.Value].ToggleSelection();
            }
            else
            {
                int step = highlightedIndex.Value < previousHighlightedIndex.Value ? -1 : 1;
                for (int i = previousHighlightedIndex.Value; i != highlightedIndex.Value; i += step)
                    items[i].ToggleSelection();
            }
        }

        MaintainInvariants();

        if (previousHighlightedItem != HighlightedItem)
        {
            OnHighlightedItemChanged(HighlightedItem?.Item);
        }
    }

    protected virtual void OnHighlightedItemChanged(T? item)
    {
        var handler = HighlightedItemChanged;
        if (handler is not null)
            handler(this, new HighlightedItemChangedEventArgs<T?>(item));
    }

    private void MaintainInvariants()
    {
        if (items.Count > 0)
        {
            if (!highlightedIndex.HasValue)
                highlightedIndex = 0;

            if (highlightedIndex < scrollOffset)
                scrollOffset = highlightedIndex.Value;
            else if (highlightedIndex >= scrollOffset + pageSize)
                scrollOffset = highlightedIndex.Value - pageSize + 1;

            if (scrollOffset + pageSize > items.Count)
                scrollOffset = Math.Max(0, items.Count - pageSize);
        }
        else
        {
            highlightedIndex = null;
            scrollOffset = 0;
        }
    }

    public void Draw(Canvas canvas, Func<T, ConsoleString> getLabel)
    {
        var itemsCanvas = canvas.GetSubArea(0, 0, canvas.Width - 1, canvas.Height);
        var scrollBarCanvas = canvas.GetSubArea(canvas.Width - 1, 0, 1, canvas.Height);

        DrawItems(itemsCanvas, getLabel);
        DrawScrollBar(scrollBarCanvas);
    }

    private void DrawItems(Canvas canvas, Func<T, ConsoleString> getLabel)
    {
        int lineIndex = 0;

        for (int itemIndex = lineIndex + scrollOffset;
            itemIndex < items.Count && lineIndex < pageSize;
            itemIndex++, lineIndex++)
        {
            var item = items[itemIndex];

            bool isHighlighted = highlightedIndex == itemIndex;
            bool isSelected = item.IsSelected;

            var backgroundColor = (isHighlighted, isSelected) switch
            {
                (true, true) => ConsoleString.CreateStyled(Theme.Instance.ItemHighlighted + Theme.Instance.ItemSelected),
                (true, false) => ConsoleString.CreateStyled(Theme.Instance.ItemHighlighted),
                (false, true) => ConsoleString.CreateStyled(Theme.Instance.ItemSelected),
                (false, false) => ConsoleString.CreateStyled(Theme.Instance.ItemNormal)
            };

            var text = getLabel(items[itemIndex].Item);
            var line = ConsoleString.Concat(backgroundColor, text);
            canvas.FillLine(lineIndex, line);
        }

        for (; lineIndex < pageSize; lineIndex++)
        {
            canvas.FillLine(lineIndex, ConsoleString.Empty);
        }
    }

    private void DrawScrollBar(Canvas canvas)
    {
        var scrollBar = ScrollBarLayout.Compute(canvas.Height, scrollOffset, pageSize, items.Count);

        for (int i = 0; i < canvas.Height; i++)
        {
            var glyph = scrollBar.GetVerticalGlyph(i);
            canvas.FillLine(i, ConsoleString.CreatePlainText(glyph.ToString()));
        }
    }

    public static ListView<string> FromAsciiArt(string inputText)
    {
        ListItem<string> ParseItem(string line)
        {
            var trimmedLine = line.Trim(' ', '>', '|');
            return trimmedLine switch
            {
                ['*', .. var rest] => new ListItem<string>(rest) { IsSelected = true },
                _ => new ListItem<string>(trimmedLine)
            };
        }

        var lines = inputText
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .ToArray();

        var items = lines.Select(ParseItem).ToList();
        var scrollOffset = Array.FindIndex(lines, line => line.EndsWith("|"));
        var pageSize = lines.Count(line => line.EndsWith("|"));
        var highlightedIndex = Array.FindIndex(lines, line => line.StartsWith(">")) switch
        {
            var x when x >= 0 => x,
            _ => (int?)null
        };

        return new ListView<string>(
            items,
            scrollOffset,
            pageSize,
            highlightedIndex,
            filterPredicate: (item, filter) => item.Contains(filter));
    }

    public string ToAsciiArt()
    {
        var result = new StringBuilder();
        var isAnyItemSelected = items.Any(item => item.IsSelected);

        for (int i = 0; i < originalItems.Count; i++)
        {
            if (i == highlightedIndex)
                result.Append("> ");
            else
                result.Append("  ");

            if (isAnyItemSelected)
            {
                if (originalItems[i].IsSelected)
                    result.Append('*');
                else
                    result.Append(' ');
            }

            result.Append(originalItems[i].Item);

            if (i >= scrollOffset && i < scrollOffset + pageSize)
                result.Append(" |");

            result.AppendLine();
        }

        return result.ToString();
    }
}
