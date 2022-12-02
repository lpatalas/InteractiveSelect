using System;
using System.Text;

namespace InteractiveSelect;

internal static class StringExtensions
{
    private static bool IsControlChar(char c)
        => char.IsControl(c) && c != '\x1b';

    public static string AddEllipsis(this string input, int maxLength)
    {
        if (input.Length > maxLength)
            return string.Concat(input.AsSpan(0, maxLength - 1), new ReadOnlySpan<char>('…'));
        else
            return input;
    }

    public static string RemoveControlSequences(
        this string input,
        bool keepSgrSequences = false)
    {
        int i = 0;

        while (i < input.Length && !char.IsControl(input[i]))
            i++;

        if (i == input.Length)
            return input;

        var result = new StringBuilder(input.Length);
        result.Append(input.AsSpan(0, i));

        while (i < input.Length)
        {
            char c = input[i];

            if (c == '\x1b')
            {
                var sequence = EscapeSequence.Parse(input.AsSpan(i));
                if (keepSgrSequences && sequence.Code == EscapeSequenceCode.Sgr)
                    result.Append(sequence.AsSpan());
                i += sequence.Length;
            }
            else
            {
                if (!char.IsControl(c))
                    result.Append(c);
                i++;
            }
        }

        return result.ToString();
    }
}
