using System;
using System.Collections.Generic;
using System.Speech.Synthesis.TtsEngine;

namespace InteractiveSelect;

internal class CollectionView<T>
    where T : class
{
    private string? filter;
    private readonly Func<T, string, bool> filterPredicate;
    private readonly List<T> items;
    private readonly IReadOnlyList<T> originalItems;

    public T this[int index] => items[index];

    public int Count => items.Count;
    public string? Filter { get => filter; set => SetFilter(value); }
    public int? HighlightedIndex { get; private set; }
    public T? HighlightedItem => HighlightedIndex.HasValue ? items[HighlightedIndex.Value] : default;
    public int PageSize { get; }
    public int ScrollOffset { get; private set; }

    public CollectionView(
        IReadOnlyList<T> originalItems,
        int pageSize,
        Func<T, string, bool> filterPredicate)
    {
        items = new List<T>(originalItems);
        this.originalItems = originalItems;
        this.filterPredicate = filterPredicate;

        HighlightedIndex = items.Count > 0 ? 0 : null;
        PageSize = pageSize;
    }

    private void SetFilter(string? filter)
    {
        this.filter = filter;

        var oldHighlightedItem = HighlightedItem;

        items.Clear();
        if (filter != null)
        {
            foreach (var item in originalItems)
            {
                if (filterPredicate(item, filter))
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
    }

    public void HighlightPreviousItem()
        => SetHighlightedIndex(HighlightedIndex - 1);

    public void HighlightNextItem()
        => SetHighlightedIndex(HighlightedIndex + 1);

    public void HighlightPreviousPage()
        => SetHighlightedIndex(HighlightedIndex - PageSize + 1);

    public void HighlightNextPage()
        => SetHighlightedIndex(HighlightedIndex + PageSize - 1);

    public void HighlightFirstItem()
        => SetHighlightedIndex(items.Count > 0 ? 0 : null);

    public void HighlightLastItem()
        => SetHighlightedIndex(items.Count > 0 ? items.Count - 1 : null);

    public void SetHighlightedIndex(int? newIndex)
    {
        if (items.Count > 0 && newIndex.HasValue)
            HighlightedIndex = Math.Clamp(newIndex.Value, 0, items.Count - 1);
        else
            HighlightedIndex = null;

        MaintainInvariants();
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
                ScrollOffset = items.Count - PageSize;
        }
        else
        {
            HighlightedIndex = null;
            ScrollOffset = 0;
        }
    }
}
