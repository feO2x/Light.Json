using System;
using System.Text;
using FluentAssertions;
using Light.Json.FrameworkExtensions;
using Light.Json.Tests.SerializationSubjects;
using Xunit;

namespace Light.Json.Tests.Serialization
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
            writer.WriteNumber();
            writer.WriteEndOfObject();

            return writer.GetUtf8Json();
        }
    }

    public struct JsonWriter
    {
        public JsonWriter(byte[] currentBuffer)
        {
            CurrentBuffer = currentBuffer;
            CurrentIndex = 0;
        }

        public byte[] CurrentBuffer { get; }

        public int CurrentIndex { get; private set; }

        public void WriteCharacter(char character) => CurrentBuffer[CurrentIndex++] = (byte) character;

        public unsafe void WriteString(ReadOnlySpan<char> value)
        {
            WriteCharacter('\"');
            fixed (char* source = &value[0])
            fixed (byte* target = &CurrentBuffer[CurrentIndex])
            {
                CurrentIndex += Encoding.UTF8.GetBytes(source, value.Length, target, CurrentBuffer.Length - CurrentIndex);
            }

            WriteCharacter('\"');
        }

        public unsafe void WriteRaw(ReadOnlySpan<byte> bytes)
        {
            fixed (void* source = &bytes[0], target = &CurrentBuffer[CurrentIndex])
                Buffer.MemoryCopy(source, target, CurrentBuffer.Length - CurrentIndex, bytes.Length);
            CurrentIndex += bytes.Length;
        }

        public void WriteNumber()
        {
            WriteCharacter('3');
            WriteCharacter('3');
        }

        public void WriteEndOfObject()
        {
            WriteCharacter('}');
        }

        public Memory<byte> GetUtf8Json() =>
            new Memory<byte>(CurrentBuffer, 0, CurrentIndex);
    }
}