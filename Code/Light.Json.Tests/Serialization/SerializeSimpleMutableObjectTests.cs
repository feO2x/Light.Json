using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Light.Json.Buffers;
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

        [Fact]
        public static async Task StreamingUtf16()
        {
            var contract = new PersonContract();
            var jsonWriter = new AsyncJsonWriter<Utf16BufferWriter>(new Utf16BufferWriter(new InMemoryArrayPoolBufferWriterService<char>()));

            await SerializePerson(jsonWriter, contract);

            var utf16Json = jsonWriter.BufferWriter.Finish();
            utf16Json.Should().Be(ExpectedJson);
        }

        [Fact]
        public static async Task StreamingUtf8()
        {
            var contract = new PersonContract();
            var jsonWriter = new AsyncJsonWriter<Utf8BufferWriter>(new Utf8BufferWriter(new InMemoryArrayPoolBufferWriterService<byte>()));

            await SerializePerson(jsonWriter, contract);

            var utf8Json = jsonWriter.BufferWriter.Finish();
            var utf16Json = utf8Json.AsSpan().ConvertFromUtf8ToString();
            utf16Json.Should().Be(ExpectedJson);
        }

        private static async Task SerializePerson<TBufferWriter>(AsyncJsonWriter<TBufferWriter> jsonWriter, PersonContract contract)
            where TBufferWriter : struct, IBufferWriter
        {
            await jsonWriter.WriteBeginOfObjectAsync();

            await jsonWriter.WriteStringAsync(contract.FirstName.Utf16);
            await jsonWriter.WriteKeyValueSeparatorAsync();
            await jsonWriter.WriteStringAsync(Person.FirstName);
            await jsonWriter.WriteValueSeparatorAsync();

            await jsonWriter.WriteStringAsync(contract.LastName.Utf16);
            await jsonWriter.WriteKeyValueSeparatorAsync();
            await jsonWriter.WriteStringAsync(Person.LastName);
            await jsonWriter.WriteValueSeparatorAsync();

            await jsonWriter.WriteStringAsync(contract.Age.Utf16);
            await jsonWriter.WriteKeyValueSeparatorAsync();
            await jsonWriter.WriteIntegerAsync(Person.Age);
            await jsonWriter.WriteEndOfObjectAsync();
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