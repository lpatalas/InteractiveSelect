using System;
using System.Collections.Generic;
using System.Linq;

namespace InteractiveSelect;

internal delegate bool ListFilterPredicate<T>(T item, string filter);

file class ListItem<T>
{
    public bool IsSelected { get; set; }
    public T Item { get; }

    public ListItem(T item)
    {
        Item = item;
    }

    public override bool Equals(object? obj)
        => ReferenceEquals(obj, this);

    public override int GetHashCode()
        => HashCode.Combine(this);

    public override string ToString()
        => nameof(ListItem<T>);
}

internal class ListView<T>
    where T : class
{
    private string filter = string.Empty;
    private readonly ListFilterPredicate<T> filterPredicate;
    private readonly List<ListItem<T>> items;
    private readonly IReadOnlyList<ListItem<T>> originalItems;
    private readonly Action<T?>? highlightedItemChangedCallback;

    public T this[int index] => items[index].Item;

    public IReadOnlyList<T> Items => items.Select(x => x.Item).ToList();

    public int Count => items.Count;
    public string Filter { get => filter; set => SetFilter(value); }
    public int? HighlightedIndex { get; private set; }
    public T? HighlightedItemValue => HighlightedIndex.HasValue ? items[HighlightedIndex.Value].Item : default;
    public int PageSize { get; }
    public int ScrollOffset { get; private set; }

    private ListItem<T>? HighlightedItem => HighlightedIndex.HasValue ? items[HighlightedIndex.Value] : default;

    public IReadOnlyList<T> OriginalItems => originalItems.Select(x => x.Item).ToList();

    public ListView(
        IReadOnlyList<T> originalItems,
        int pageSize,
        ListFilterPredicate<T> filterPredicate,
        Action<T?>? highlightedItemChangedCallback)
        : this(
              originalItems,
              scrollOffset: 0,
              pageSize,
              highlightedIndex: originalItems.Count > 0 ? 0 : null,
              filterPredicate)
    {
        this.highlightedItemChangedCallback = highlightedItemChangedCallback;
    }

    public ListView(
        IReadOnlyList<T> originalItems,
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

        this.originalItems = originalItems.Select(x => new ListItem<T>(x)).ToList();
        this.items = new List<ListItem<T>>(this.originalItems);
        this.filterPredicate = filterPredicate;

        HighlightedIndex = highlightedIndex;
        PageSize = Math.Min(items.Count, pageSize);
        ScrollOffset = scrollOffset;
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
            }
        }
        else
        {
            items.AddRange(originalItems);
        }

        if (oldHighlightedItem != null)
        {
            int newHighlightedIndex = items.IndexOf(oldHighlightedItem);
            HighlightedIndex = newHighlightedIndex >= 0 ? newHighlightedIndex : null;
        }

        MaintainInvariants();

        if (HighlightedItem != oldHighlightedItem)
        {
            OnHighlightedItemChanged(HighlightedItemValue);
        }
    }

    public void HighlightPreviousItem()
        => SetHighlightedIndex(HighlightedIndex - 1);

    public void HighlightNextItem()
        => SetHighlightedIndex(HighlightedIndex + 1);

    public void HighlightItemPageUp()
        => SetHighlightedIndex(HighlightedIndex - PageSize + 1);

    public void HighlightItemPageDown()
        => SetHighlightedIndex(HighlightedIndex + PageSize - 1);

    public void HighlightFirstItem()
        => SetHighlightedIndex(items.Count > 0 ? 0 : null);

    public void HighlightLastItem()
        => SetHighlightedIndex(items.Count > 0 ? items.Count - 1 : null);

    public void SetHighlightedIndex(int? newIndex)
    {
        var previousHighlightedItem = HighlightedItemValue;

        if (items.Count > 0 && newIndex.HasValue)
            HighlightedIndex = Math.Clamp(newIndex.Value, 0, items.Count - 1);
        else
            HighlightedIndex = null;

        MaintainInvariants();

        if (previousHighlightedItem != HighlightedItemValue)
        {
            OnHighlightedItemChanged(HighlightedItemValue);
        }
    }

    protected virtual void OnHighlightedItemChanged(T? item)
    {
        highlightedItemChangedCallback?.Invoke(item);
    }

    private void MaintainInvariants()
    {
        if (items.Count > 0)
        {
            if (!HighlightedIndex.HasValue)
                HighlightedIndex = 0;

            if (HighlightedIndex < ScrollOffset)
                ScrollOffset = HighlightedIndex.Value;
            else if (HighlightedIndex >= ScrollOffset + PageSize)
                ScrollOffset = HighlightedIndex.Value - PageSize + 1;

            if (ScrollOffset + PageSize > items.Count)
                ScrollOffset = Math.Max(0, items.Count - PageSize);
        }
        else
        {
            HighlightedIndex = null;
            ScrollOffset = 0;
        }
    }
}
