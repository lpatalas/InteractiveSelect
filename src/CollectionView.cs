using System;
using System.Collections.Generic;

namespace InteractiveSelect;

internal class CollectionView<T>
{
    private readonly List<T> items;
    private readonly IReadOnlyList<T> originalItems;

    public T this[int index] => items[index];

    public int Count => items.Count;
    public int HighlightedIndex { get; private set; }
    public int PageSize { get; }
    public int ScrollOffset { get; private set; }

    public CollectionView(IReadOnlyList<T> originalItems, int pageSize)
    {
        items = new List<T>(originalItems);
        this.originalItems = originalItems;
        PageSize = pageSize;
    }

    public void SetHighlightedIndex(int newIndex)
    {
        HighlightedIndex = Math.Clamp(newIndex, 0, items.Count - 1);
        if (HighlightedIndex < ScrollOffset)
            ScrollOffset = HighlightedIndex;
        else if (HighlightedIndex >= ScrollOffset + PageSize)
            ScrollOffset = HighlightedIndex - PageSize + 1;
    }
}
