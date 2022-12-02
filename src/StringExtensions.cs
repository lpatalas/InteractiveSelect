using System;
using System.Diagnostics;

namespace InteractiveSelect;

internal static class StringExtensions
{
    public static string AddEllipsis(this string input, int maxLength)
    {
        Debug.Assert(input != null);
        Debug.Assert(maxLength >= 0);

        if (maxLength < 1)
            return string.Empty;
        else if (input.Length > maxLength)
            return string.Concat(input.AsSpan(0, maxLength - 1), new ReadOnlySpan<char>('…'));
        else
            return input;
    }
}
