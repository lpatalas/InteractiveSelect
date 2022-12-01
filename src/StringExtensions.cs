using System;
using System.Diagnostics;
using System.Text;

namespace InteractiveSelect;

internal static class StringExtensions
{
    private static bool IsControlChar(char c)
        => char.IsControl(c) && c != '\x1b';

    public static string RemoveControlCharactersExceptEsc(this string input)
    {
        int? firstControlCharIndex = null;

        for (int i = 0; i < input.Length; i++)
        {
            if (IsControlChar(input[i]))
            {
                firstControlCharIndex = i;
                break;
            }
        }

        if (firstControlCharIndex.HasValue)
        {
            var result = new StringBuilder();
            result.Append(input, 0, firstControlCharIndex.Value);
            for (int i = firstControlCharIndex.Value + 1; i < input.Length; i++)
            {
                if (!IsControlChar(input[i]))
                    result.Append(input[i]);
            }

            return result.ToString();
        }
        else
        {
            return input;
        }
    }

    public static string RemoveControlSequences(this string input)
    {
        int i = 0;

        while (i < input.Length && !char.IsControl(input[i]))
            i++;

        if (i == input.Length)
            return input;

        var result = new StringBuilder(input.Length);
        result.Append(input.AsSpan(0, i));

        static void SkipControlSequence(string input, ref int i)
        {
            Debug.Assert(i < input.Length - 1);
            Debug.Assert(input[i] == '\x1b');
            Debug.Assert(input[i + 1] == '[');

            i += 2; // Skip leading "ESC[" pair

            while (i < input.Length && (input[i] < '\x40' || input[i] > '\x7e'))
            {
                i++;
            }

            if (i < input.Length)
                i++; // Skip sequence terminator
        }

        while (i < input.Length)
        {
            char c = input[i];

            if (c == '\x1b')
            {
                if (i < input.Length - 1 && input[i + 1] == '[')
                    SkipControlSequence(input, ref i);
                else
                    i++; // Skip lone ESC character at the end of the input
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
