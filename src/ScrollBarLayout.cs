﻿using System;
using System.Text;

namespace InteractiveSelect;

internal readonly record struct ScrollBarLayout(
    int LeadingBarSize,
    int ThumbSize,
    int TrailingBarSize)
{
    public const char HorizontalBarChar = '\u2500';
    public const char HorizontalThumbChar = '\u2501';
    public const char VerticalBarChar = '\u2502';
    public const char VerticalThumbChar = '\u2503';

    public int TotalSize => LeadingBarSize + ThumbSize + TrailingBarSize;

    public static ScrollBarLayout Compute(
        int scrollBarSize,
        int scrollOffset,
        int pageSize,
        int totalCount)
    {
        if (totalCount < 1)
            return new ScrollBarLayout(0, scrollBarSize, 0);

        var visibleItemCount = Math.Min(totalCount, pageSize);
        var thumbStart = DivRound(scrollOffset * scrollBarSize, totalCount);
        var thumbSize = Math.Max(1, DivRound(visibleItemCount * scrollBarSize, totalCount));

        return new ScrollBarLayout(
            thumbStart,
            thumbSize,
            Math.Max(0, scrollBarSize - thumbSize - thumbStart));
    }

    private static int DivRound(int a, int b)
        => (a + b / 2) / b;

    public char GetVerticalGlyph(int offset)
    {
        if (offset < LeadingBarSize || offset >= LeadingBarSize + ThumbSize)
            return VerticalBarChar;
        else
            return VerticalThumbChar;
    }

    public void Render(StringBuilder output, char barChar, char thumbChar)
    {
        output.Append(barChar, LeadingBarSize);
        output.Append(thumbChar, ThumbSize);
        output.Append(barChar, TrailingBarSize);
    }
}
