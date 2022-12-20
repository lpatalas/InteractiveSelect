using FluentAssertions;
using Xunit;

namespace InteractiveSelect.Tests;

public class EscapeSequenceTests
{
    [Theory]
    [InlineData("", "")]
    [InlineData("X", "")] // Not starting with ESC
    [InlineData("\b", "")] // Different control char than ESC
    [InlineData("\x1b", "\x1b")] // Lone ESC
    [InlineData("\x001b9", "\x1b")] // Not a sequence (9 is not in 0x40-0x5f range)
    [InlineData("\x001bA", "\x1b")] // Unsupported sequence
    [InlineData("\x1b[m", "\x1b[m")] // Sequence ending exactly at the end of input
    [InlineData("\x1b[mText", "\x1b[m")] // Additional text after sequence
    [InlineData("\x1b[", "\x1b[")] // Sequence not terminated properly
    [InlineData("\x1b[0123", "\x1b[0123")] // Sequence not terminated properly
    [InlineData("\x1b]8;;http://example.com\x1b\\Text", "\x1b]8;;http://example.com\x1b\\")]
    [InlineData("\x1b]8;;http://example.com\bText", "\x1b]8;;http://example.com\b")]
    public void TestParsing(string input, string expected)
    {
        var sequence = EscapeSequence.Parse(input);
        sequence.ToString().Should().Be(expected);
    }
}
