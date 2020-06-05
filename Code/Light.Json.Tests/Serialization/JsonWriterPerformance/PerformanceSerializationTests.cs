using System;
using System.Text;
using FluentAssertions;
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
        public static readonly byte[] WriterBuffer = new byte[256];

        [Fact]
        public static void SerializeUtf8()
        {
            var person = Person.CreateDefaultInstance();

            var result = Serialize(person);

            result.Span.ConvertFromUtf8ToString().Should().Be(Person.DefaultMinifiedJson);
        }

        public static Memory<byte> Serialize(Person person)
        {
            var writer = new JsonWriter(WriterBuffer);
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