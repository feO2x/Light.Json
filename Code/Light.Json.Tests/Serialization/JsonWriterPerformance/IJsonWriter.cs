using System;

namespace Light.Json.Tests.Serialization.JsonWriterPerformance
{
    public interface IJsonWriter
    {
        void WriteString(ReadOnlySpan<char> value);

        void WriteRaw(ReadOnlySpan<byte> bytes);

        void WriteNumber(long number);

        void WriteEndOfObject();
    }
}