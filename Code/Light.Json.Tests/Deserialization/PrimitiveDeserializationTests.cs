using System;
using FluentAssertions;
using Light.Json.Tokenization.Utf8;
using Xunit;

namespace Light.Json.Tests.Deserialization
{
    public static class PrimitiveDeserializationTests
    {
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