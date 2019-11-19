using System;
using FluentAssertions;
using Light.Json.Tokenization.Utf8;
using Xunit;
using Xunit.Abstractions;

namespace Light.Json.Tests.Deserialization
{
    public sealed class PrimitiveDeserializationTests
    {
        private readonly ITestOutputHelper _output;

        public PrimitiveDeserializationTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [MemberData(nameof(JsonStringData))]
        public static void DeserializeJsonStringUtf16(string json, string expected)
        {
            var result = DeserializeUtf16<string>(json);

            result.Should().Be(expected);
        }

        [Theory]
        [MemberData(nameof(JsonStringData))]
        public static void DeserializeJsonStringUtf8(string json, string expected)
        {
            var result = DeserializeUtf8<string>(json);

            result.Should().Be(expected);
        }

        public static readonly TheoryData<string, string> JsonStringData =
            new TheoryData<string, string>
            {
                { "\"Foo\"", "Foo" },
                { "\"Bar\"", "Bar" },
                { "  \"Baz\" ", "Baz" },
                { "\t\"Qux\"", "Qux" },
                { "\"K\\u0065nny\"", "Kenny" }
            };

        [Theory]
        [MemberData(nameof(JsonInt32Data))]
        public static void DeserializeInt32Utf16(string json, int expected)
        {
            var result = DeserializeUtf16<int>(json);

            result.Should().Be(expected);
        }

        [Theory]
        [MemberData(nameof(JsonInt32Data))]
        public static void DeserializeInt32Utf8(string json, int expected)
        {
            var result = DeserializeUtf8<int>(json);

            result.Should().Be(expected);
        }

        public static readonly TheoryData<string, int> JsonInt32Data =
            new TheoryData<string, int>
            {
                { "1", 1 },
                { "3", 3 },
                { "-99", -99 },
                { "0", 0 },
                { "3.0", 3 },
                { "0.000", 0 },
                { int.MaxValue.ToString(), int.MaxValue },
                { int.MinValue.ToString(), int.MinValue }
            };

        [Theory]
        [MemberData(nameof(InvalidInt32Data))]
        public void InvalidIntegerValuesUtf16(string json)
        {
            Action act = () => DeserializeUtf16<int>(json);

            act.Should().Throw<DeserializationException>().Which.ShouldBeWrittenTo(_output);
        }

        [Theory]
        [MemberData(nameof(InvalidInt32Data))]
        public void InvalidIntegerValuesUtf8(string json)
        {
            Action act = () => DeserializeUtf8<int>(json);

            act.Should().Throw<DeserializationException>().Which.ShouldBeWrittenTo(_output);
        }

        public static readonly TheoryData<string> InvalidInt32Data =
            new TheoryData<string>
            {
                "2147483648",
                "-2147483649",
                "Not an int value at all"
            };

        private static T DeserializeUtf16<T>(string json)
        {
            var deserializer = new JsonDeserializer();
            return deserializer.Deserialize<T>(json.AsMemory());
        }

        private static T DeserializeUtf8<T>(string json)
        {
            var deserializer = new JsonDeserializer();
            return deserializer.Deserialize<T>(json.ToUtf8());
        }
    }
}