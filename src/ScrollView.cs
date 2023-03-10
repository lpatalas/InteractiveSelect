using System;
using System.Collections.Generic;

namespace InteractiveSelect;

internal class ScrollView<T>
{
    private IReadOnlyList<T> items;
    private int pageSize;
    private int scrollOffset;

    public int TotalCount => items.Count;

    public ScrollView(int pageSize)
    {
        this.items = Array.Empty<T>();
        this.pageSize = pageSize;
    }

    public ScrollView(IReadOnlyList<T> items, int pageSize)
    {
        this.items = items;
        this.pageSize = pageSize;
    }

    public Page<T> GetCurrentPage()
        => new Page<T>(items, scrollOffset, Math.Min(items.Count, pageSize));

    public void SetPageSize(int newPageSize)
    {
        pageSize = newPageSize;
        if (scrollOffset + pageSize >= items.Count)
        {
            scrollOffset = Math.Max(0, items.Count - pageSize);
        }
    }

    public void SetItems(IReadOnlyList<T> items)
    {
        this.items = items;
        scrollOffset = 0;
    }

    public void ScrollDown()
    {
        scrollOffset = Math.Min(
            scrollOffset + 1,
            Math.Max(0, items.Count - pageSize));
    }

    public void ScrollUp()
    {
        scrollOffset = Math.Max(0, scrollOffset - 1);
    }

    public void ScrollPageDown()
    {
        scrollOffset = Math.Min(
            scrollOffset + pageSize,
            Math.Max(0, items.Count - pageSize));
    }

    public void ScrollPageUp()
    {
        scrollOffset = Math.Max(0, scrollOffset - pageSize);
    }

    public void ScrollToTheTop()
    {
        scrollOffset = 0;
    }

    public void ScrollToTheBottom()
    {
        scrollOffset = Math.Max(0, items.Count - pageSize);
    }
}
