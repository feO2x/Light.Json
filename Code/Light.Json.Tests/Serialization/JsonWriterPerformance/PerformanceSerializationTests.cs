using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using FluentAssertions;
using Light.Json.Buffers;
using Light.Json.FrameworkExtensions;
using Light.Json.Tests.SerializationSubjects;
using Xunit;

namespace Light.Json.Tests.Serialization.JsonWriterPerformance
{
    public static class PerformanceSerializationTests
    {
        public static readonly IBufferProvider<byte> BufferProvider = new ThreadStaticByteBufferProvider();

        public static readonly PersonContract PersonContract = new PersonContract();

        public static readonly Dictionary<TypeKey, BaseContract> Contracts =
            new Dictionary<TypeKey, BaseContract>(TypeKey.EqualityComparer.Instance)
            {
                [typeof(Person)] = new PersonContract()
            };

        [Fact]
        public static void SerializeUtf8()
        {
            var person = Person.CreateDefaultInstance();

            using var result = Serialize(person);

            result.Json.Span.ConvertFromUtf8ToString().Should().Be(Person.DefaultMinifiedJson);
        }

        public static SerializationResult<byte> Serialize<T>(T value)
        {
            var writer = new JsonWriter(BufferProvider);
            var context = new SerializationContext();
            SerializeViaContract(value, context, ref writer);
            return writer.GetUtf8Json();
        }

        private static void SerializeViaContract<T, TJsonWriter>(T value, SerializationContext context, ref TJsonWriter writer)
            where TJsonWriter : struct, IJsonWriter
        {
            if (typeof(T) == typeof(Person))
            {
                PersonContract.Serialize(Unsafe.As<T, Person>(ref value), context, ref writer);
                return;
            }

            throw new SerializationException("Unknown contract type");
        }
    }
}