﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace InteractiveSelect;

internal readonly struct ConsoleString
{
    private readonly string? value;

    private bool HasEscapeSequences => ContentLength != (value?.Length ?? 0);

    public int ContentLength { get; }

    public static readonly ConsoleString Empty = new ConsoleString();

    private ConsoleString(string value, int contentLength)
    {
        Debug.Assert(contentLength <= value.Length);

        this.ContentLength = contentLength;
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
            return new ConsoleString(input, input.Length);

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

        return new ConsoleString(result.ToString(), contentLength);
    }

    public ConsoleString AddEllipsis(int maxLength)
    {
        if (maxLength < 1)
            return ConsoleString.Empty;

        if (value == null || ContentLength <= maxLength || value.Length <= maxLength)
            return this;

        if (!HasEscapeSequences)
        {
            var truncatedString = value.AddEllipsis(maxLength);
            return new ConsoleString(
                truncatedString,
                contentLength: truncatedString.Length);
        }

        int i = 0;
        int contentLength = 0;

        while (i < value.Length && contentLength < maxLength - 1)
        {
            if (value[i] == '\x1b')
            {
                var sequence = EscapeSequence.Parse(value.AsSpan(i));
                i += sequence.Length;
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
            contentLength: maxLength);
    }

    public IReadOnlyList<ConsoleString> WordWrap(int lineLength)
    {
        Debug.Assert(lineLength > 0);

        if (value == null || ContentLength <= lineLength)
            return new[] { this };

        var results = new List<ConsoleString>();
        var currentLine = new StringBuilder(capacity: lineLength);
        var currentLineLength = 0;
        var i = 0;
        int? lengthUntilLastSpace = null;
        int? contentLengthUntilLastSpace = null;
        bool hasContent = false;
        
        while (i < value.Length)
        {
            if (value[i] == '\x1b')
            {
                var sequence = EscapeSequence.Parse(value.AsSpan(i));
                currentLine.Append(sequence.AsSpan());
                i += sequence.Length;
            }
            else
            {
                currentLine.Append(value[i]);
                currentLineLength++;

                if (char.IsWhiteSpace(value[i]))
                {
                    if (hasContent)
                    {
                        lengthUntilLastSpace = currentLine.Length;
                        contentLengthUntilLastSpace = currentLineLength;
                    }
                }
                else
                {
                    hasContent = true;
                }

                i++;
            }

            if (currentLineLength == lineLength)
            {
                Debug.Assert(lengthUntilLastSpace.HasValue == contentLengthUntilLastSpace.HasValue);

                var isNextCharSpace = i < value.Length && char.IsWhiteSpace(value[i]);

                if (lengthUntilLastSpace.HasValue
                    && contentLengthUntilLastSpace.HasValue
                    && !isNextCharSpace)
                {
                    var usedPart = currentLine.ToString(0, lengthUntilLastSpace.Value);
                    results.Add(new ConsoleString(usedPart, lengthUntilLastSpace.Value));
                    currentLine.Remove(0, lengthUntilLastSpace.Value);
                    currentLineLength -= contentLengthUntilLastSpace.Value;
                }
                else
                {
                    results.Add(new ConsoleString(currentLine.ToString(), currentLineLength));
                    currentLine.Clear();
                    currentLineLength = 0;
                    hasContent = false;
                }

                lengthUntilLastSpace = null;
                contentLengthUntilLastSpace = null;
            }
        }

        if (currentLineLength > 0)
        {
            results.Add(new ConsoleString(currentLine.ToString(), currentLineLength));
        }

        return results;
    }

    // TODO: This can give incorrect answer if string contains escape sequences
    public bool Contains(string substring, StringComparison comparison)
        => value?.Contains(substring, comparison) ?? false;

    public static ConsoleString Concat(ConsoleString first, ConsoleString second)
    {
        var concatenatedValues = string.Concat(first.value, second.value);
        return new ConsoleString(
            concatenatedValues,
            contentLength: first.ContentLength + second.ContentLength);
    }

    public static ConsoleString Concat(
        ConsoleString s1,
        ConsoleString s2,
        ConsoleString s3,
        ConsoleString s4)
    {
        var concatenatedValues = string.Concat(s1.value, s2.value, s3.value, s4.value);
        return new ConsoleString(
            concatenatedValues,
            contentLength: s1.ContentLength + s2.ContentLength + s3.ContentLength + s4.ContentLength);
    }

    public ConsoleString ToPlainText()
        => HasEscapeSequences ? CreatePlainText(value ?? string.Empty) : this;

    public override bool Equals(object? obj)
        => obj is ConsoleString other
        && string.Equals(value, other.value, StringComparison.Ordinal);

    public override int GetHashCode()
        => value?.GetHashCode() ?? 0;

    public override string ToString()
        => value ?? string.Empty;
}
