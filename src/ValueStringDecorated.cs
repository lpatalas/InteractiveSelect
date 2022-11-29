using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Automation;
using System.Text;
using System.Text.RegularExpressions;

namespace InteractiveSelect;

// Adapted from: https://github.com/PowerShell/PowerShell/blob/019c3b87df68d970a98f88124c46a66179361016/src/System.Management.Automation/FormatAndOutput/common/StringDecorated.cs
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
internal partial struct ValueStringDecorated
{
    internal const char ESC = '\x1b';
    private readonly bool _isDecorated;
    private readonly string _text;
    private string? _plaintextcontent;
    private Dictionary<int, int>? _vtRanges;

    private string PlainText
    {
        get
        {
            _plaintextcontent ??= AnsiRegex().Replace(_text, string.Empty);

            return _plaintextcontent;
        }
    }

    // graphics/color mode ESC[1;2;...m
    private const string GraphicsRegex = @"(\x1b\[(\d+(;\d+)*)?m)";

    // CSI escape sequences
    private const string CsiRegex = @"(\x1b\[\?\d+[hl])";

    // Hyperlink escape sequences. Note: '.*?' makes '.*' do non-greedy match.
    private const string HyperlinkRegex = @"(\x1b\]8;;.*?\x1b\\)";

    private const string ControlSequenceRegex = @"\p{Cc}+";

    // replace regex with .NET 6 API once available
    [GeneratedRegex($"{GraphicsRegex}|{CsiRegex}|{HyperlinkRegex}|{ControlSequenceRegex}")]
    private static partial Regex AnsiRegex();

    /// <summary>
    /// Get the ranges of all escape sequences in the text.
    /// </summary>
    /// <returns>
    /// A dictionary with the key being the starting index of an escape sequence,
    /// and the value being the length of the escape sequence.
    /// </returns>
    internal Dictionary<int, int>? EscapeSequenceRanges
    {
        get
        {
            if (_isDecorated && _vtRanges is null)
            {
                _vtRanges = new Dictionary<int, int>();
                foreach (Match match in AnsiRegex().Matches(_text))
                {
                    _vtRanges.Add(match.Index, match.Length);
                }
            }

            return _vtRanges;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueStringDecorated"/> struct.
    /// </summary>
    /// <param name="text">The input string.</param>
    public ValueStringDecorated(string text)
    {
        _text = text;
        _isDecorated = text.Contains(ESC);
        _plaintextcontent = _isDecorated ? null : text;
        _vtRanges = null;
    }

    /// <summary>
    /// Gets a value indicating whether the string contains decoration.
    /// </summary>
    /// <returns>Boolean if the string contains decoration.</returns>
    public bool IsDecorated => _isDecorated;

    /// <summary>
    /// Gets the length of content sans escape sequences.
    /// </summary>
    /// <returns>Length of content sans escape sequences.</returns>
    public int ContentLength => PlainText.Length;

    public ValueStringDecorated AddEllipsis(int maxLength)
    {
        if (ContentLength <= maxLength)
            return this;

        var resultBuilder = new StringBuilder();

        if (EscapeSequenceRanges is not null)
        {
            int resultContentLength = 0;
            int i = 0;
            while (resultContentLength < maxLength - 1)
            {
                int sequenceLength;
                if (EscapeSequenceRanges.TryGetValue(i, out sequenceLength))
                {
                    resultBuilder.Append(_text, i, sequenceLength);
                    i += sequenceLength;
                }
                else
                {
                    resultBuilder.Append(_text[i]);
                    resultContentLength++;
                    i++;
                }
            }
        }
        else
        {
            resultBuilder.Append(_text, 0, maxLength - 1);
        }

        resultBuilder.Append('…');

        var result = new ValueStringDecorated(resultBuilder.ToString());
        Debug.Assert(result.ContentLength == maxLength);
        return result;
    }

    /// <summary>
    /// Render the decorarted string using automatic output rendering.
    /// </summary>
    /// <returns>Rendered string based on automatic output rendering.</returns>
    public override string ToString() => ToString(
        PSStyle.Instance.OutputRendering == OutputRendering.PlainText
            ? OutputRendering.PlainText
            : OutputRendering.Ansi);

    /// <summary>
    /// Return string representation of content depending on output rendering mode.
    /// </summary>
    /// <param name="outputRendering">Specify how to render the text content.</param>
    /// <returns>Rendered string based on outputRendering.</returns>
    public string ToString(OutputRendering outputRendering)
    {
        if (outputRendering == OutputRendering.Host)
        {
            throw new ArgumentException("Only 'ANSI' or 'PlainText' is supported for this method.");
        }

        if (!_isDecorated)
        {
            return _text;
        }

        return outputRendering == OutputRendering.PlainText ? PlainText : _text;
    }
}
