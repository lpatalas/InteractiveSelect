using System.Linq;
using FluentAssertions;
using Xunit;

namespace InteractiveSelect.Tests;

public class ConsoleStringTests
{
    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("text text")]
    public void TestInputWithoutControlCharacters(string input)
    {
        var plainTextString = ConsoleString.CreatePlainText(input);
        var styledString = ConsoleString.CreateStyled(input);

        styledString.Should().Be(plainTextString);

        plainTextString.ToString().Should().BeSameAs(input);
        styledString.ToString().Should().BeSameAs(input);

        plainTextString.ContentLength.Should().Be(input.Length);
        styledString.ContentLength.Should().Be(input.Length);
    }

    [Theory]
    [InlineData("\r\n\b\x1b\v", "")]
    [InlineData("so\rme\tte\x1bxt", "sometext")]
    public void TestStringWithControlCharacters(string input, string expected)
    {
        var plainTextString = ConsoleString.CreatePlainText(input);
        var styledString = ConsoleString.CreateStyled(input);

        styledString.Should().Be(plainTextString);
        plainTextString.ToString().Should().Be(expected);
        styledString.ToString().Should().Be(expected);
    }

    [Theory]
    [InlineData("\x1b[30m\x1b\x1b[?25h", "")]
    [InlineData("\x1b[1;31mred \x1b[mand \x1b[30;47mblack\u001b[0m", "red and black")]
    public void PlainTextShouldRemoveControlSequences(string input, string expected)
    {
        var plainTextString = ConsoleString.CreatePlainText(input);

        plainTextString.ToString().Should().Be(expected);
        plainTextString.ContentLength.Should().Be(expected.Length);
    }

    [Theory]
    [InlineData("text", "text", 4)]
    [InlineData("\x1b[1;31mred", "\x1b[1;31mred", 3)]
    [InlineData("(\u001b[?25h)(\x1b[1;31m)(\x1b[1B)", "()(\x1b[1;31m)()", 6)]
    public void StyledStringShouldKeepSgrControlSequences(
        string input,
        string expected,
        int expectedContentLength)
    {
        var styledString = ConsoleString.CreateStyled(input);

        styledString.ToString().Should().Be(expected);
        styledString.ContentLength.Should().Be(expectedContentLength);
    }

    [Theory]
    [InlineData("\x1b", "")]
    [InlineData("\x1b[", "")]
    [InlineData("\x1b[0000", "")]
    [InlineData("lone\x001bescape", "loneescape")]
    [InlineData("escattheend\x1b", "escattheend")]
    [InlineData("csiattheend\x1b[", "csiattheend")]
    public void ShouldHandleVariousEdgeCases(string input, string expected)
    {
        var plainTextString = ConsoleString.CreatePlainText(input);

        plainTextString.ToString().Should().Be(expected);
        plainTextString.ContentLength.Should().Be(expected.Length);
    }

    [Theory]
    [InlineData("", 0, "")]
    [InlineData("", 10, "")]
    [InlineData("t\x1b[1;31mest", 0, "")]
    [InlineData("t\x1b[1;31mest", 1, "…")]
    [InlineData("t\x1b[1;31mest", 2, "t…")]
    [InlineData("t\x1b[1;31mest", 3, "t\x1b[1;31me…")]
    [InlineData("t\x1b[1;31mest", 4, "t\x1b[1;31mest")]
    [InlineData("t\x1b[1;31mest", 5, "t\x1b[1;31mest")]
    public void AddEllipsisTests(string input, int maxLength, string expected)
    {
        var styledString = ConsoleString.CreateStyled(input);
        var result = styledString.AddEllipsis(maxLength);

        result.ToString().Should().Be(expected);
    }

    [Theory]
    [InlineData("t|e|x|t", 1)]
    [InlineData("te|xt", 2)]
    [InlineData("tex|t", 3)]
    [InlineData("text", 4)]
    [InlineData("text", 5)]

    [InlineData("abc| ab|c", 3)]
    [InlineData("abc |abc", 5)]
    [InlineData("abc abc", 7)]

    [InlineData("abc abc| abc", 7)]
    [InlineData("abc abc |abc", 8)]
    [InlineData("abc abc |abc", 9)]

    [InlineData("  |  ", 2)]
    [InlineData(" a|b", 2)]
    [InlineData("  | a|b", 2)]
    [InlineData("a | b", 2)]
    public void WordWrapTests(string pattern, int lineLength)
    {
        var inputString = pattern.Replace("|", "");
        var expected = pattern.Split('|').Select(ConsoleString.CreatePlainText).ToList();

        var input = ConsoleString.CreatePlainText(inputString);
        var result = input.WordWrap(lineLength);

        result.Should().BeEquivalentTo(expected, opt => opt.ComparingByMembers<ConsoleString>());
    }
}
