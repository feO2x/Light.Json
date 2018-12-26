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
            var token = GetSingleToken(json);

            token.Type.Should().Be(JsonTokenType.String);
            token.Text.ToString().Should().Be(json);
        }

        [Fact]
        public static void TokenizeFalse()
        {
            var token = GetSingleToken("false");

            token.Type.Should().Be(JsonTokenType.False);
            token.Text.Length.Should().Be(0);
        }

        [Fact]
        public static void TokenizeTrue()
        {
            var token = GetSingleToken("true");

            token.Type.Should().Be(JsonTokenType.True);
            token.Text.Length.Should().Be(0);
        }

        [Fact]
        public static void TokenizeNull()
        {
            var token = GetSingleToken("null");

            token.Type.Should().Be(JsonTokenType.Null);
            token.Text.Length.Should().Be(0);
        }

        private static JsonToken GetSingleToken(string json)
        {
            var tokenizer = new InMemoryTokenizer(json);

            var token = tokenizer.GetNextToken();

            var secondToken = tokenizer.GetNextToken();
            secondToken.Type.Should().Be(JsonTokenType.EndOfDocument);
            secondToken.Text.Length.Should().Be(0);
            return token;
        }
    }
}