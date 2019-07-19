using FluentAssertions;
using Light.Json.Tokenization;
using Light.Json.Tokenization.Utf8;
using Xunit;

namespace Light.Json.Tests.Tokenization.Utf8
{
    public static class Utf8SymbolByteLengthTests
    {
        [Theory]
        [InlineData(UnicodeConstants.SingleLineCommentCharacter, 1)]
        [InlineData(UnicodeConstants.CarriageReturn, 1)]
        [InlineData(UnicodeConstants.NextLine, 2)]
        public static void ByteLengthOf(char character, int expectedLength) =>
            character.ToUtf8().Length.Should().Be(expectedLength);
    }
}