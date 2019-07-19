using System;
using FluentAssertions;
using Light.Json.Tokenization;
using Light.Json.Tokenization.Utf8;
using Xunit;

namespace Light.Json.Tests.Tokenization.Utf8
{
    public static class JsonUtf8SpanTokenizerTests
    {
        [Theory]
        [InlineData("\"Foo\"", "Foo")]
        [InlineData("\"Bar\"", "Bar")]
        [InlineData("\"Baz\"", "Baz")]
        [InlineData("  \"Qux\"", "Qux")]
        [InlineData("\"Qux\" ", "Qux")]
        [InlineData("\t\"Gorge\"", "Gorge")]
        [InlineData("\"\\\"Boom\\\"\"", "\\\"Boom\\\"")]
        [InlineData("\"Boom\"", "Boom")]
        public static void TokenizeJsonString(string json, string expected) =>
            TestTokenizer(json, expected, JsonTokenType.String);

        [Theory]
        [InlineData("false")]
        [InlineData(" false")]
        [InlineData("false ")]
        [InlineData("\tfalse")]
        public static void TokenizeFalse(string json) =>
            TestTokenizer(json, JsonSymbols.False, JsonTokenType.False);

        [Theory]
        [InlineData("true")]
        [InlineData("\ttrue")]
        [InlineData(" true")]
        public static void TokenizeTrue(string json) =>
            TestTokenizer(json, JsonSymbols.True, JsonTokenType.True);

        private static void TestTokenizer(string json, string expected, JsonTokenType expectedTokenType) =>
            GetSingleToken(json).ShouldEqual(expected.ToUtf8(), expectedTokenType);

        private static JsonUtf8SpanToken GetSingleToken(string json)
        {
            var utf8Json = json.ToUtf8();
            var tokenizer = new JsonUtf8SpanTokenizer(utf8Json);

            var token = tokenizer.GetNextToken();

            var secondToken = tokenizer.GetNextToken();
            secondToken.Type.Should().Be(JsonTokenType.EndOfDocument);
            secondToken.Text.Length.Should().Be(0);
            return token;
        }

        private static void ShouldEqual(this JsonUtf8SpanToken token, ReadOnlySpan<byte> expected, JsonTokenType tokenType)
        {
            token.Type.Should().Be(tokenType);
            token.Text.MustEqual(expected);
        }
    }
}