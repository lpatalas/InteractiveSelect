using System.Text.RegularExpressions;

namespace InteractiveSelect;

internal static partial class StringExtensions
{
    #region ANSI Regexes

    // Regexes taken from: https://github.com/PowerShell/PowerShell/blob/019c3b87df68d970a98f88124c46a66179361016/src/System.Management.Automation/FormatAndOutput/common/StringDecorated.cs
    // Copyright (c) Microsoft Corporation.
    // Licensed under the MIT License.

    // graphics/color mode ESC[1;2;...m
    private const string GraphicsRegex = @"(\x1b\[\d+(;\d+)*m)";

    // CSI escape sequences
    private const string CsiRegex = @"(\x1b\[\?\d+[hl])";

    // Hyperlink escape sequences. Note: '.*?' makes '.*' do non-greedy match.
    private const string HyperlinkRegex = @"(\x1b\]8;;.*?\x1b\\)";

    #endregion

    private const string ControlSequenceRegex = @"\p{Cc}";

    [GeneratedRegex($"{ControlSequenceRegex}|{GraphicsRegex}|{CsiRegex}|{HyperlinkRegex}")]
    private static partial Regex GetAnsiSequenceAndControlCharacterRegex();

    public static string RemoveEscapeSequencesAndControlCharacters(this string input)
        => GetAnsiSequenceAndControlCharacterRegex().Replace(input, string.Empty);
}
