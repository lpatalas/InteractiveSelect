using System.Text;
using FluentAssertions;
using Xunit;

namespace InteractiveSelect.Tests;

public class ScrollBarLayoutTests
{
    [Theory]
    //          size, offset, page, total
    [InlineData(   1,      0,    5,    10, "X")]
    [InlineData(   2,      0,    5,    10, "X-")]
    [InlineData(   2,      5,    5,    10, "-X")]
    [InlineData(   6,      0,    5,     0, "XXXXXX")]
    [InlineData(   6,      0,    1,     1, "XXXXXX")]
    [InlineData(   6,      0,    5,     5, "XXXXXX")]
    [InlineData(   6,      0,   10,     5, "XXXXXX")]
    [InlineData(   6,      0,    5,    10, "XXX---")]
    [InlineData(   6,      1,    5,    10, "-XXX--")]
    [InlineData(   6,      2,    5,    10, "-XXX--")]
    [InlineData(   6,      3,    5,    10, "--XXX-")]
    [InlineData(   6,      4,    5,    10, "--XXX-")]
    [InlineData(   6,      5,    5,    10, "---XXX")]
    [InlineData(  19,      9,   19,    28, "------XXXXXXXXXXXXX")]
    public void RunTests(
        int scrollBarSize, int scrollOffset, int pageSize, int totalCount, string expected)
    {
        var layout = ScrollBarLayout.Compute(scrollBarSize, scrollOffset, pageSize, totalCount);
        var result = new StringBuilder();
        layout.Render(result, barChar: '-', thumbChar: 'X');

        result.ToString().Should().Be(expected);
    }
}
