using FluentAssertions;
using Xunit;

namespace InteractiveSelect.Tests;

public class StringExtensionTests
{
    [Theory]
    [InlineData("", 0, "")]
    [InlineData("", 10, "")]
    [InlineData("test", 0, "")]
    [InlineData("test", 1, "…")]
    [InlineData("test", 2, "t…")]
    [InlineData("test", 3, "te…")]
    [InlineData("test", 4, "test")]
    [InlineData("test", 5, "test")]
    public void AddEllipsisTests(string input, int maxLength, string expected)
    {
        var result = input.AddEllipsis(maxLength);
        result.Should().Be(expected);
    }
}
