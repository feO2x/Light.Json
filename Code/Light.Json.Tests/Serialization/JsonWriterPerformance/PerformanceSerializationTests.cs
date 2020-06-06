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
        public static readonly PersonContract Contract = new PersonContract();

        [Fact]
        public static void SerializeUtf8()
        {
            var person = Person.CreateDefaultInstance();

            using var result = Serialize(person);

            result.Json.Span.ConvertFromUtf8ToString().Should().Be(Person.DefaultMinifiedJson);
        }

        public static SerializationResult<byte> Serialize(Person person)
        {
            var writer = new JsonWriter(BufferProvider);
            var context = new SerializationContext();
            Contract.Serialize(person, context, ref writer);
            return writer.GetUtf8Json();
        }
    }
}