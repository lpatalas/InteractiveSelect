using System;
using System.Collections.Generic;

namespace InteractiveSelect;

internal readonly struct Page<T>
{
    private readonly IReadOnlyList<T> items;
    private readonly int offset;
    private readonly int count;

    public T this[int index] => index < count
        ? items[offset + index]
        : throw new ArgumentOutOfRangeException(nameof(index));

    public int Count => count;

    public Page(IReadOnlyList<T> items, int offset, int count)
    {
        this.items = items;
        this.offset = offset;
        this.count = count;
    }
}
