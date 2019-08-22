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

        [Fact]
        public static void TokenizeBeginOfObject() => TestTokenizer("{", JsonTokenType.BeginOfObject);

        [Fact]
        public static void TokenizeEndOfObject() => TestTokenizer("}", JsonTokenType.EndOfObject);

        [Fact]
        public static void TokenizeBeginOfArray() => TestTokenizer("[", JsonTokenType.BeginOfArray);

        [Fact]
        public static void TokenizeEndOfArray() => TestTokenizer("]", JsonTokenType.EndOfArray);

        [Fact]
        public static void TokenizeEntrySeparator() => TestTokenizer(",", JsonTokenType.EntrySeparator);

        [Theory]
        [InlineData("fals", "false", "fals", 1)]
        [InlineData("\tfal", "false", "fal", 2)]
        [InlineData("  fa", "false", "fa", 3)]
        [InlineData("f", "false", "f", 1)]
        [InlineData("tru", "true", "tru", 1)]
        [InlineData("\ttr", "true", "tr", 2)]
        [InlineData("    t", "true", "t", 5)]
        [InlineData("nul", "null", "nul", 1)]
        [InlineData("\tnu", "null", "nu", 2)]
        [InlineData("   n", "null", "n", 4)]
        public static void InvalidConstants(string invalidJson, string expectedToken, string invalidToken, int position)
        {
            Action act = () => GetSingleToken(invalidJson);

            act.Should().ThrowExactly<DeserializationException>()
               .And.Message.Should().Contain($"Expected token \"{expectedToken}\" but actually found \"{invalidToken}\" at line 1 position {position}.");
        }

        [Theory]
        [InlineData("-", 1)]
        [InlineData("\t-f", 2)]
        [InlineData("  -$", 3)]
        public static void InvalidNegativeNumber(string invalidJson, int position)
        {
            Action act = () => GetSingleToken(invalidJson);

            act.Should().ThrowExactly<DeserializationException>()
               .And.Message.Should().Contain($"Expected digit after minus sign at line 1 position {position}.");
        }

        [Theory]
        [InlineData("15.z", 1)]
        [InlineData("184583.", 1)]
        [InlineData("-42.§", 1)]
        [InlineData("  89.", 3)]
        [InlineData("\t-321.p", 2)]
        [InlineData("-321.\t", 1)]
        public static void InvalidFloatingPointNumber(string invalidJson, int position)
        {
            Action act = () => GetSingleToken(invalidJson);

            act.Should().ThrowExactly<DeserializationException>()
               .And.Message.Should().Contain($"Expected digit after decimal symbol in \"{invalidJson.TrimStart()}\" at line 1 position {position}.");
        }

        private static void TestTokenizer(string json, JsonTokenType expectedTokenType) =>
            GetSingleToken(json).ShouldEqual(json.Trim(), expectedTokenType);

        private static void TestTokenizer(string json, string expected, JsonTokenType expectedTokenType) =>
            GetSingleToken(json).ShouldEqual(expected, expectedTokenType);

        private static JsonUtf8Token GetSingleToken(string json)
        {
            var utf8Json = json.ToUtf8();
            var tokenizer = new JsonUtf8Tokenizer(utf8Json);

            var token = tokenizer.GetNextToken();

            var secondToken = tokenizer.GetNextToken();
            secondToken.Type.Should().Be(JsonTokenType.EndOfDocument);
            secondToken.ByteSequence.Length.Should().Be(0);
            return token;
        }

        private static void ShouldEqual(this JsonUtf8Token token, string expected, JsonTokenType tokenType)
        {
            token.Type.Should().Be(tokenType);
            token.NumberOfCharacters.Should().Be(expected.Length);
            token.ByteSequence.MustEqual(expected.ToUtf8());
        }
    }
}