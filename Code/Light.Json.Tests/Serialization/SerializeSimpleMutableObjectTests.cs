using System;
using System.Collections.Generic;
using FluentAssertions;
using Light.Json.Contracts;
using Light.Json.Serialization;
using Light.Json.Serialization.LowLevelWriting;
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
                new ImmutableSerializationContractProvider(
                    new Dictionary<TypeKey, ISerializationContract>
                    {
                        [typeof(Person)] = new PersonContract()
                    }
                ),
                new ArrayPoolBufferProvider<char>(),
                new ArrayPoolBufferProvider<byte>()
            );

        [Fact]
        public void SerializeSimpleObjectUtf16()
        {
            var json = _serializer.SerializeToUtf16(Person);
            json.Should().Be(ExpectedJson);
        }

        [Fact]
        public void SerializeSimpleObjectUtf8()
        {
            var json = _serializer.SerializeToUtf8(Person);
            var utf16Json = json.AsSpan().ConvertFromUtf8ToString();
            utf16Json.Should().Be(ExpectedJson);
        }

        public sealed class PersonContract : SerializeOnlyContract<Person>
        {
            public readonly ContractConstant Age = nameof(Person.Age);
            public readonly ContractConstant FirstName = nameof(Person.FirstName);
            public readonly ContractConstant LastName = nameof(Person.LastName);

            public override void Serialize<TJsonWriter>(Person person, SerializationContext context, ref TJsonWriter writer)
            {
                writer.WriteBeginOfObject();

                writer.WriteContractConstantAsObjectKey(FirstName);
                writer.WriteKeyValueSeparator();
                writer.WriteString(person.FirstName.AsSpan());
                writer.WriteValueSeparator();

                writer.WriteContractConstantAsObjectKey(LastName);
                writer.WriteKeyValueSeparator();
                writer.WriteString(person.LastName.AsSpan());
                writer.WriteValueSeparator();

                writer.WriteContractConstantAsObjectKey(Age);
                writer.WriteKeyValueSeparator();
                writer.WriteInteger(person.Age);

                writer.WriteEndOfObject();
            }
        }
    }
}