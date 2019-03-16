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
        [InlineData("  \"Qux\"")]
        [InlineData("\"Qux\" ")]
        [InlineData("\t\"Gorge\"")]
        public static void TokenizeJsonString(string json) =>
            TestTokenizer(json, json.Trim(), JsonTokenType.String);

        [Theory]
        [InlineData("false")]
        [InlineData(" false")]
        [InlineData("false ")]
        [InlineData("\tfalse")]
        public static void TokenizeFalse(string json) =>
            TestTokenizer(json, JsonTokenizerSymbols.False, JsonTokenType.False);

        [Theory]
        [InlineData("true")]
        [InlineData("\ttrue")]
        [InlineData(" true")]
        public static void TokenizeTrue(string json) =>
            TestTokenizer(json, JsonTokenizerSymbols.True, JsonTokenType.True);

        [Theory]
        [InlineData("null")]
        [InlineData(" null")]
        [InlineData("\tnull")]
        public static void TokenizeNull(string json) =>
            TestTokenizer(json, JsonTokenizerSymbols.Null, JsonTokenType.Null);

        [Theory]
        [InlineData(42)]
        [InlineData(8)]
        [InlineData(1006)]
        [InlineData(17853)]
        [InlineData(-35)]
        [InlineData(-108)]
        [InlineData(0)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public static void TokenizeIntegerNumber(int number) =>
            TestTokenizer(number.ToString(), JsonTokenType.IntegerNumber);

        [Theory]
        [InlineData("42.75")]
        [InlineData("  745237823932.472392")]
        [InlineData("\t-150.2299")]
        [InlineData("-0.2")]
        public static void TokenizeFloatingPointNumber(string numberAsJson) =>
            TestTokenizer(numberAsJson, numberAsJson.TrimStart(), JsonTokenType.FloatingPointNumber);

        [Fact]
        public static void TokenizeBeginOfObject() => TestTokenizer("{", JsonTokenType.BeginOfObject);

        [Fact]
        public static void TokenizeEndOfObject() => TestTokenizer("}", JsonTokenType.EndOfObject);

        [Fact]
        public static void TokenizeBeginOfArray() => TestTokenizer("[", JsonTokenType.BeginOfArray);

        [Fact]
        public static void TokenizeEndOfArray() => TestTokenizer("]", JsonTokenType.EndOfArray);

        private static void TestTokenizer(string json, JsonTokenType expectedTokenType) =>
            GetSingleToken(json).ShouldEqual(json, expectedTokenType);

        private static void TestTokenizer(string json, string expectedToken, JsonTokenType expectedTokenType) =>
            GetSingleToken(json).ShouldEqual(expectedToken, expectedTokenType);

        private static JsonToken GetSingleToken(string json)
        {
            var tokenizer = new InMemoryTokenizer(json);

            var token = tokenizer.GetNextToken();

            var secondToken = tokenizer.GetNextToken();
            secondToken.Type.Should().Be(JsonTokenType.EndOfDocument);
            secondToken.Text.Length.Should().Be(0);
            return token;
        }

        private static void ShouldEqual(this JsonToken token, string expected, JsonTokenType tokenType)
        {
            token.Type.Should().Be(tokenType);
            token.Text.ToString().Should().Be(expected);
        }
    }
}