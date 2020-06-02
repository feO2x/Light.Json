using System;
using System.Collections.Generic;
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

        public sealed class PersonContract : SerializeOnlyContract<Person>
        {
            public readonly ConstantValue Age = nameof(Person.Age);
            public readonly ConstantValue FirstName = nameof(Person.FirstName);
            public readonly ConstantValue LastName = nameof(Person.LastName);
            public readonly ConstantValue Segment1;
            public readonly ConstantValue Segment2;
            public readonly ConstantValue Segment3;

            public PersonContract()
            {
                Segment1 = ConstantValue.Unaltered("{\"" + FirstName + "\":");
                Segment2 = ConstantValue.Unaltered(",\"" + LastName + "\":");
                Segment3 = ConstantValue.Unaltered(",\"" + Age + "\":");
            }

            public override void Serialize<TJsonWriter>(Person person, SerializationContext context, ref TJsonWriter writer)
            {
                if (writer.IsCompatibleWithOptimizedContract)
                {
                    SerializeFast(person, ref writer);
                    return;
                }

                writer.WriteBeginOfObject();

                writer.WriteConstantValueAsObjectKey(FirstName);
                writer.WriteKeyValueSeparator();
                writer.WriteString(person.FirstName.AsSpan());
                writer.WriteValueSeparator();

                writer.WriteConstantValueAsObjectKey(LastName);
                writer.WriteKeyValueSeparator();
                writer.WriteString(person.LastName.AsSpan());
                writer.WriteValueSeparator();

                writer.WriteConstantValueAsObjectKey(Age);
                writer.WriteKeyValueSeparator();
                writer.WriteInteger(person.Age);

                writer.WriteEndOfObject();
            }

            private void SerializeFast<TJsonWriter>(Person person, ref TJsonWriter writer)
                where TJsonWriter : struct, IJsonWriter
            {
                writer.WriteConstantValueLarge(Segment1);
                writer.WriteString(person.FirstName.AsSpan());
                writer.WriteConstantValueLarge(Segment2);
                writer.WriteString(person.LastName.AsSpan());
                writer.WriteConstantValue7(Segment3);
                writer.WriteInteger(person.Age);
                writer.WriteEndOfObject();
            }
        }
    }
}