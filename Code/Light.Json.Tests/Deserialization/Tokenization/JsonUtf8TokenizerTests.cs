﻿using System;
using System.Runtime.Serialization;
using FluentAssertions;
using Light.Json.Deserialization.Tokenization;
using Light.Json.Deserialization.Tokenization.Utf8;
using Light.Json.FrameworkExtensions;
using Xunit;

namespace Light.Json.Tests.Deserialization.Tokenization
{
    public static class JsonUtf8TokenizerTests
    {
        [Theory]
        [InlineData("\"Foo\"")]
        [InlineData("\"Bar\"")]
        [InlineData("\"Baz\"")]
        [InlineData("  \"Qux\"")]
        [InlineData("\"Qux\" ")]
        [InlineData("\t\"Gorge\"")]
        [InlineData("\"\\\"Boom\\\"\"")]
        [InlineData("\"Boom\"")]
        public static void TokenizeJsonString(string json) =>
            TestTokenizer(json, JsonTokenType.String);

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

            act.Should().ThrowExactly<SerializationException>()
               .And.Message.Should().Contain($"Expected token \"{expectedToken}\" but actually found \"{invalidToken}\" at line 1 position {position}.");
        }

        [Theory]
        [InlineData("-", 1)]
        [InlineData("\t-f", 2)]
        [InlineData("  -$", 3)]
        public static void InvalidNegativeNumber(string invalidJson, int position)
        {
            Action act = () => GetSingleToken(invalidJson);

            act.Should().ThrowExactly<SerializationException>()
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

            act.Should().ThrowExactly<SerializationException>()
               .And.Message.Should().Contain($"Expected digit after decimal symbol in \"{invalidJson.TrimStart()}\" at line 1 position {position}.");
        }

        [Fact]
        public static void TokenizeNameValueSeparator() =>
            TestTokenizer(":", JsonTokenType.NameValueSeparator);

        [Theory]
        [InlineData("\\", '\\', 1, 1)]
        [InlineData("\r\n\\", '\\', 2, 1)]
        public static void InvalidObjectAndArrayTokens(string invalidJson, char invalidCharacter, int line, int position)
        {
            Action act = () => GetSingleToken(invalidJson);

            act.Should().ThrowExactly<SerializationException>()
               .And.Message.Should().Contain($"Unexpected character \"{invalidCharacter}\" at line {line} position {position}.");
        }

        [Fact]
        public static void TokenizeComplexObject()
        {
            const string json = @"
{
    ""firstName"": ""John"",
    ""lastName"": ""Doe"",
    ""age"": 42
}";
            var jsonInUtf8 = json.ToUtf8();
            var tokenizer = new JsonUtf8Tokenizer(jsonInUtf8);

            tokenizer.GetNextToken().ShouldEqual("{", JsonTokenType.BeginOfObject);
            tokenizer.GetNextToken().ShouldEqual("\"firstName\"", JsonTokenType.String);
            tokenizer.GetNextToken().ShouldEqual(":", JsonTokenType.NameValueSeparator);
            tokenizer.GetNextToken().ShouldEqual("\"John\"", JsonTokenType.String);
            tokenizer.GetNextToken().ShouldEqual(",", JsonTokenType.EntrySeparator);
            tokenizer.GetNextToken().ShouldEqual("\"lastName\"", JsonTokenType.String);
            tokenizer.GetNextToken().ShouldEqual(":", JsonTokenType.NameValueSeparator);
            tokenizer.GetNextToken().ShouldEqual("\"Doe\"", JsonTokenType.String);
            tokenizer.GetNextToken().ShouldEqual(",", JsonTokenType.EntrySeparator);
            tokenizer.GetNextToken().ShouldEqual("\"age\"", JsonTokenType.String);
            tokenizer.GetNextToken().ShouldEqual(":", JsonTokenType.NameValueSeparator);
            tokenizer.GetNextToken().ShouldEqual("42", JsonTokenType.IntegerNumber);
            tokenizer.GetNextToken().ShouldEqual("}", JsonTokenType.EndOfObject);
            tokenizer.GetNextToken().ShouldEqual("", JsonTokenType.EndOfDocument);
        }

        [Fact]
        public static void TokenizeArray()
        {
            const string json = @"
[
    ""This is a JSON string"",
    true,
    false,
    null,
    {
        ""this is"": ""a complex object""
    },
    78
]
";
            var jsonInUtf8 = json.ToUtf8();
            var tokenizer = new JsonUtf8Tokenizer(jsonInUtf8);

            tokenizer.GetNextToken().ShouldEqual("[", JsonTokenType.BeginOfArray);
            tokenizer.GetNextToken().ShouldEqual("\"This is a JSON string\"", JsonTokenType.String);
            tokenizer.GetNextToken().ShouldEqual(",", JsonTokenType.EntrySeparator);
            tokenizer.GetNextToken().ShouldEqual("true", JsonTokenType.True);
            tokenizer.GetNextToken().ShouldEqual(",", JsonTokenType.EntrySeparator);
            tokenizer.GetNextToken().ShouldEqual("false", JsonTokenType.False);
            tokenizer.GetNextToken().ShouldEqual(",", JsonTokenType.EntrySeparator);
            tokenizer.GetNextToken().ShouldEqual("null", JsonTokenType.Null);
            tokenizer.GetNextToken().ShouldEqual(",", JsonTokenType.EntrySeparator);
            tokenizer.GetNextToken().ShouldEqual("{", JsonTokenType.BeginOfObject);
            tokenizer.GetNextToken().ShouldEqual("\"this is\"", JsonTokenType.String);
            tokenizer.GetNextToken().ShouldEqual(":", JsonTokenType.NameValueSeparator);
            tokenizer.GetNextToken().ShouldEqual("\"a complex object\"", JsonTokenType.String);
            tokenizer.GetNextToken().ShouldEqual("}", JsonTokenType.EndOfObject);
            tokenizer.GetNextToken().ShouldEqual(",", JsonTokenType.EntrySeparator);
            tokenizer.GetNextToken().ShouldEqual("78", JsonTokenType.IntegerNumber);
            tokenizer.GetNextToken().ShouldEqual("]", JsonTokenType.EndOfArray);
            tokenizer.GetNextToken().ShouldEqual("", JsonTokenType.EndOfDocument);
        }

        [Fact]
        public static void IgnoreSingleLineComments()
        {
            const string json = @"
// This is a single line comment at the beginning of the file
{
    ""someCollection"": [
        // Here is a comment within a collection
        42,
        18,
        30
    ]
}";
            var jsonInUtf8 = json.ToUtf8();
            var tokenizer = new JsonUtf8Tokenizer(jsonInUtf8);

            tokenizer.GetNextToken().ShouldEqual("{", JsonTokenType.BeginOfObject);
            tokenizer.GetNextToken().ShouldEqual("\"someCollection\"", JsonTokenType.String);
            tokenizer.GetNextToken().ShouldEqual(":", JsonTokenType.NameValueSeparator);
            tokenizer.GetNextToken().ShouldEqual("[", JsonTokenType.BeginOfArray);
            tokenizer.GetNextToken().ShouldEqual("42", JsonTokenType.IntegerNumber);
            tokenizer.GetNextToken().ShouldEqual(",", JsonTokenType.EntrySeparator);
            tokenizer.GetNextToken().ShouldEqual("18", JsonTokenType.IntegerNumber);
            tokenizer.GetNextToken().ShouldEqual(",", JsonTokenType.EntrySeparator);
            tokenizer.GetNextToken().ShouldEqual("30", JsonTokenType.IntegerNumber);
            tokenizer.GetNextToken().ShouldEqual("]", JsonTokenType.EndOfArray);
            tokenizer.GetNextToken().ShouldEqual("}", JsonTokenType.EndOfObject);
            tokenizer.GetNextToken().ShouldEqual("", JsonTokenType.EndOfDocument);
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
            secondToken.Memory.Length.Should().Be(0);
            return token;
        }

        private static void ShouldEqual(this JsonUtf8Token token, string expected, JsonTokenType tokenType)
        {
            token.Type.Should().Be(tokenType);
            token.Memory.Span.MustEqual(expected.ToUtf8());
        }
    }
}