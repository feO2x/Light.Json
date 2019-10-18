using FluentAssertions;
using Light.Json.Tokenization;
using Light.Json.Tokenization.Utf8;
using Xunit;

namespace Light.Json.Tests.Tokenization.Utf8
{
    public static class Utf8ByteLengthTests
    {
        [Theory]
        [InlineData(JsonSymbols.SingleLineCommentCharacter, 1)]
        [InlineData(JsonSymbols.LineFeed, 1)]
        [InlineData(JsonSymbols.CarriageReturn, 1)]
        [InlineData(JsonSymbols.NextLine, 2)]
        public static void ByteLengthOfCharacter(char character, int expectedLength) =>
            character.ToUtf8().Length.Should().Be(expectedLength);

        [Theory]
        [InlineData(JsonSymbols.False, 5)]
        public static void ByteLengthOfString(string @string, int expectedLength) =>
             @string.ToUtf8().Length.Should().Be(expectedLength);
    }
}