using System.Collections.Generic;
using FluentAssertions;
using Light.Json.Contracts;
using Light.Json.FrameworkExtensions;
using Light.Json.Tests.SerializationSubjects;
using Xunit;

namespace Light.Json.Tests.Serialization
{
    public sealed class SerializeSimpleMutableObjectTests
    {
        private const string ExpectedJson = "{\"firstName\":\"Kenny\",\"lastName\":\"Pflug\",\"age\":33}";
        private static readonly Person Person = new Person { FirstName = "Kenny", LastName = "Pflug", Age = 33 };

        private readonly JsonSerializer _serializer =
            new JsonSerializer(
                new JsonSerializerSettings
                {
                    ContractProvider = new ImmutableContractProvider(
                        new Dictionary<TypeKey, ISerializationContract> { [typeof(Person)] = new PersonContract() }
                    )
                }
            );

        [Fact]
        public void SerializeSimpleObjectUtf16()
        {
            var result = _serializer.SerializeToUtf16(Person);
            result.GetJsonAsStringAndDisposeResult().Should().Be(ExpectedJson);
        }

        [Fact]
        public void SerializeSimpleObjectUtf8()
        {
            using var result = _serializer.SerializeToUtf8(Person);
            var utf16Json = result.Json.Span.ConvertFromUtf8ToString();
            utf16Json.Should().Be(ExpectedJson);
        }
    }
}