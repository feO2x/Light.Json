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
        public static void TokenizeJsonString(string json)
        {
            var token = GetSingleToken(json);

            token.Type.Should().Be(JsonTokenType.String);
            token.Text.ToString().Should().Be(json.Trim());
        }

        [Theory]
        [InlineData("false")]
        [InlineData(" false")]
        [InlineData("false ")]
        [InlineData("\tfalse")]
        public static void TokenizeFalse(string json)
        {
            var token = GetSingleToken(json);

            token.Type.Should().Be(JsonTokenType.False);
            token.Text.ToString().Should().Be(JsonTokenizerSymbols.False);
        }

        [Theory]
        [InlineData("true")]
        [InlineData("\ttrue")]
        [InlineData(" true")]
        public static void TokenizeTrue(string json)
        {
            var token = GetSingleToken(json);

            token.Type.Should().Be(JsonTokenType.True);
            token.Text.ToString().Should().Be(JsonTokenizerSymbols.True);
        }

        [Theory]
        [InlineData("null")]
        [InlineData(" null")]
        [InlineData("\tnull")]
        public static void TokenizeNull(string json)
        {
            var token = GetSingleToken(json);

            token.Type.Should().Be(JsonTokenType.Null);
            token.Text.ToString().Should().Be(JsonTokenizerSymbols.Null);
        }

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
        public static void TokenizeIntegerNumber(int number)
        {
            var jsonNumber = number.ToString();
            var token = GetSingleToken(jsonNumber);

            token.Type.Should().Be(JsonTokenType.IntegerNumber);
            token.Text.ToString().Should().Be(jsonNumber);
        }

        [Theory]
        [InlineData("42.75")]
        [InlineData("  745237823932.472392")]
        [InlineData("\t-150.2299")]
        [InlineData("-0.2")]
        public static void TokenizeFloatingPointNumber(string numberAsJson)
        {
            var token = GetSingleToken(numberAsJson);

            token.Type.Should().Be(JsonTokenType.FloatingPointNumber);
            token.Text.ToString().Should().Be(numberAsJson.TrimStart());
        }

        [Fact]
        public static void TokenizeBeginOfObject()
        {
            var token = GetSingleToken("{");

            token.Type.Should().Be(JsonTokenType.BeginOfObject);
            token.Text.ToString().Should().Be("{");
        }

        [Fact]
        public static void TokenizeEndOfObject()
        {
            var token = GetSingleToken("}");

            token.Type.Should().Be(JsonTokenType.EndOfObject);
            token.Text.ToString().Should().Be("}");
        }

        [Fact]
        public static void TokenizeBeginOfArray()
        {
            var token = GetSingleToken("[");

            token.Type.Should().Be(JsonTokenType.BeginOfArray);
            token.Text.ToString().Should().Be("[");
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