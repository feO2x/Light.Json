using System;
using System.Text;
using FluentAssertions;
using Light.Json.Buffers;
using Light.Json.FrameworkExtensions;
using Light.Json.Tests.SerializationSubjects;
using Xunit;

namespace Light.Json.Tests.Serialization.JsonWriterPerformance
{
    public static class PerformanceSerializationTests
    {
        public static readonly byte[] Segment1 = Encoding.UTF8.GetBytes("{\"firstName\":");
        public static readonly byte[] Segment2 = Encoding.UTF8.GetBytes(",\"lastName\":");
        public static readonly byte[] Segment3 = Encoding.UTF8.GetBytes(",\"age\":");
        public static readonly IBufferProvider<byte> BufferProvider = new ThreadStaticByteBufferProvider();

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
            writer.WriteRaw(Segment1);
            writer.WriteString(person.FirstName.AsSpan());
            writer.WriteRaw(Segment2);
            writer.WriteString(person.LastName.AsSpan());
            writer.WriteRaw(Segment3);
            writer.WriteNumber(person.Age);
            writer.WriteEndOfObject();

            return writer.GetUtf8Json();
        }
    }
}