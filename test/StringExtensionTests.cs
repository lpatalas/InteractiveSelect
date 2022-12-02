using Xunit;

namespace InteractiveSelect.Tests;

public class StringExtensionTests
{
    public class RemoveControlSequencesTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("a")]
        [InlineData("text text")]
        public void ShouldReturnInputInstanceWhenItDoesNotContainAnyControlSequences(string input)
        {
            var result = input.RemoveControlSequences();
            Assert.Same(input, result);
        }

        [Theory]
        [InlineData("\r\n\b\x1b\v", "")]
        [InlineData("so\rme\tte\x1bxt", "sometext")]
        public void ShouldRemoveAllControlCharacters(string input, string expected)
        {
            var result = input.RemoveControlSequences();
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("\x1b[30m\x1b\x1b[?25h", "")]
        [InlineData("\x1b[1;31mred \x1b[mand \x1b[30;47mblack\u001b[0m", "red and black")]
        public void ShouldRemoveControlSequences(string input, string expected)
        {
            var result = input.RemoveControlSequences();
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("text", "text")]
        [InlineData("\x1b[1;31mred", "\x1b[1;31mred")]
        [InlineData("(\u001b[?25h)(\x1b[1;31m)(\x1b[1B)", "()(\x1b[1;31m)()")]
        public void ShouldKeepSgrControlSequencesIfSuchFlagIsSet(string input, string expected)
        {
            var result = input.RemoveControlSequences(keepSgrSequences: true);
            Assert.Equal(expected, result);
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
            var result = input.RemoveControlSequences();
            Assert.Equal(expected, result);
        }
    }
}
