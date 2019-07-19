using FluentAssertions;
using Light.Json.Tokenization;
using Xunit;

namespace Light.Json.Tests.Tokenization
{
    public static class Utf8SymbolByteLengthTests
    {
        [Theory]
        [InlineData(ImportantUnicode.SingleLineCommentCharacter, 1)]
        [InlineData(ImportantUnicode.CarriageReturn, 1)]
        [InlineData(ImportantUnicode.NextLine, 2)]
        public static void ByteLengthOf(char character, int expectedLength) =>
            character.ToUtf8().Length.Should().Be(expectedLength);
    }
}