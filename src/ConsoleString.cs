using System;
using System.Text;

namespace InteractiveSelect;

internal readonly struct ConsoleString
{
    private readonly bool hasEscapeSequences;
    private readonly string? value;

    public int ContentLength { get; }

    public static readonly ConsoleString Empty = new ConsoleString();

    private ConsoleString(
        string value,
        int contentLength,
        bool hasEscapeSequences)
    {
        this.ContentLength = contentLength;
        this.hasEscapeSequences = hasEscapeSequences;
        this.value = value;
    }

    public static ConsoleString CreatePlainText(string input)
        => ParseString(input, keepSgrSequences: false);

    public static ConsoleString CreateStyled(string input)
        => ParseString(input, keepSgrSequences: true);

    private static ConsoleString ParseString(string input, bool keepSgrSequences = false)
    {
        int i = 0;

        while (i < input.Length && !char.IsControl(input[i]))
            i++;

        if (i == input.Length)
            return new ConsoleString(input, input.Length, hasEscapeSequences: false);

        bool hasEscapeSequences = false;
        int contentLength = i;

        var result = new StringBuilder(input.Length);
        result.Append(input.AsSpan(0, i));

        while (i < input.Length)
        {
            char c = input[i];

            if (c == '\x1b')
            {
                var sequence = EscapeSequence.Parse(input.AsSpan(i));
                if (keepSgrSequences && sequence.Code == EscapeSequenceCode.Sgr)
                {
                    result.Append(sequence.AsSpan());
                    hasEscapeSequences = true;
                }

                i += sequence.Length;
            }
            else
            {
                if (!char.IsControl(c))
                {
                    result.Append(c);
                    contentLength++;
                }

                i++;
            }
        }

        return new ConsoleString(result.ToString(), contentLength, hasEscapeSequences);
    }

    public ConsoleString AddEllipsis(int maxLength)
    {
        if (maxLength < 1)
            return ConsoleString.Empty;

        if (value == null || ContentLength <= maxLength || value.Length <= maxLength)
            return this;

        if (!hasEscapeSequences)
        {
            var truncatedString = value.AddEllipsis(maxLength);
            return new ConsoleString(
                truncatedString,
                contentLength: truncatedString.Length,
                hasEscapeSequences: false);
        }

        int i = 0;
        int contentLength = 0;
        bool foundEscapeSequence = false;

        while (i < value.Length && contentLength < maxLength - 1)
        {
            if (value[i] == '\x1b')
            {
                var sequence = EscapeSequence.Parse(value.AsSpan(i));
                i += sequence.Length;
                foundEscapeSequence = true;
            }
            else
            {
                contentLength++;
                i++;
            }
        }

        if (i == value.Length)
            return this;

        return new ConsoleString(
            string.Concat(value.AsSpan(0, i), new ReadOnlySpan<char>('…')),
            contentLength: maxLength,
            hasEscapeSequences: foundEscapeSequence);
    }

    // TODO: This can give incorrect answer if string contains escape sequences
    public bool Contains(string substring, StringComparison comparison)
        => value?.Contains(substring, comparison) ?? false;

    public static ConsoleString Concat(ConsoleString first, ConsoleString second)
    {
        var concatenatedValues = string.Concat(first.value, second.value);
        return new ConsoleString(
            concatenatedValues,
            contentLength: first.ContentLength + second.ContentLength,
            hasEscapeSequences: first.hasEscapeSequences || second.hasEscapeSequences);
    }

    public ConsoleString ToPlainText()
        => hasEscapeSequences ? CreatePlainText(value ?? string.Empty) : this;

    public override bool Equals(object? obj)
        => obj is ConsoleString other
        && string.Equals(value, other.value, StringComparison.Ordinal);

    public override int GetHashCode()
        => value?.GetHashCode() ?? 0;

    public override string ToString()
        => value ?? string.Empty;
}
