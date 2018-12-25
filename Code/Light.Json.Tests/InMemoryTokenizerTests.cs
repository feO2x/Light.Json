using FluentAssertions;
using Xunit;

namespace Light.Json.Tests
{
    public static class InMemoryTokenizerTests
    {
        [Theory]
        [InlineData("\"Foo\"")]
        [InlineData("\"Bar\"")]
        [InlineData("\"Baz\"")]
        public static void TokenizeJsonString(string json)
        {
            var tokenizer = new InMemoryTokenizer(json);

            var token = tokenizer.GetNextToken();

            token.Type.Should().Be(JsonTokenType.String);
            token.Text.ToString().Should().Be(json);
        }
    }
}