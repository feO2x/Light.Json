using System;
using FluentAssertions;
using Light.Json.Tokenization;
using Light.Json.Tokenization.Utf8;
using Xunit;

namespace Light.Json.Tests.Tokenization.Utf8
{
    public static class JsonUtf8TokenizerTests
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

        [Theory]
        [InlineData("null")]
        [InlineData(" null")]
        [InlineData("\tnull")]
        public static void TokenizeNull(string json) =>
            TestTokenizer(json, JsonSymbols.Null, JsonTokenType.Null);

        [Theory]
        [InlineData("42")]
        [InlineData("8")]
        [InlineData(" 1006")]
        [InlineData("17853 ")]
        [InlineData("\t-35")]
        [InlineData("-108 ")]
        [InlineData("0")]
        [InlineData("-2147483648")]
        [InlineData("2147483647")]
        public static void TokenizeIntegerNumber(string json) =>
            TestTokenizer(json, JsonTokenType.IntegerNumber);

        [Theory]
        [InlineData("42.75")]
        [InlineData("  745237823932.472392")]
        [InlineData("\t-150.2299")]
        [InlineData("-0.2")]
        [InlineData("0.7375 ")]
        public static void TokenizeFloatingPointNumber(string numberAsJson) =>
            TestTokenizer(numberAsJson, JsonTokenType.FloatingPointNumber);

        private static void TestTokenizer(string json, JsonTokenType expectedTokenType) =>
            GetSingleToken(json).ShouldEqual(json.Trim().ToUtf8(), expectedTokenType);

        private static void TestTokenizer(string json, string expected, JsonTokenType expectedTokenType) =>
            GetSingleToken(json).ShouldEqual(expected.ToUtf8(), expectedTokenType);

        private static JsonUtf8Token GetSingleToken(string json)
        {
            var utf8Json = json.ToUtf8();
            var tokenizer = new JsonUtf8Tokenizer(utf8Json);

            var token = tokenizer.GetNextToken();

            var secondToken = tokenizer.GetNextToken();
            secondToken.Type.Should().Be(JsonTokenType.EndOfDocument);
            secondToken.Text.Length.Should().Be(0);
            return token;
        }

        private static void ShouldEqual(this JsonUtf8Token token, ReadOnlySpan<byte> expected, JsonTokenType tokenType)
        {
            token.Type.Should().Be(tokenType);
            token.Text.MustEqual(expected);
        }
    }
}