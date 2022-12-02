using System.Management.Automation.Language;
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

        Assert.Equal(plainTextString, styledString);
        Assert.Same(input, plainTextString.ToString());
        Assert.Same(input, styledString.ToString());
        Assert.Equal(input.Length, plainTextString.ContentLength);
        Assert.Equal(input.Length, styledString.ContentLength);
    }

    [Theory]
    [InlineData("\r\n\b\x1b\v", "")]
    [InlineData("so\rme\tte\x1bxt", "sometext")]
    public void TestStringWithControlCharacters(string input, string expected)
    {
        var plainTextString = ConsoleString.CreatePlainText(input);
        var styledString = ConsoleString.CreateStyled(input);

        Assert.Equal(plainTextString, styledString);
        Assert.Equal(expected, plainTextString.ToString());
        Assert.Equal(expected, styledString.ToString());
    }

    [Theory]
    [InlineData("\x1b[30m\x1b\x1b[?25h", "")]
    [InlineData("\x1b[1;31mred \x1b[mand \x1b[30;47mblack\u001b[0m", "red and black")]
    public void PlainTextShouldRemoveControlSequences(string input, string expected)
    {
        var plainTextString = ConsoleString.CreatePlainText(input);
        
        Assert.Equal(expected, plainTextString.ToString());
        Assert.Equal(expected.Length, plainTextString.ContentLength);
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

        Assert.Equal(expected, styledString.ToString());
        Assert.Equal(expectedContentLength, styledString.ContentLength);
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
        Assert.Equal(expected, plainTextString.ToString());
        Assert.Equal(expected.Length, plainTextString.ContentLength);
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

        Assert.Equal(expected, result.ToString());
    }
}
