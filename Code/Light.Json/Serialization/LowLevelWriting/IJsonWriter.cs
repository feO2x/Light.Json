using System;

namespace Light.Json.Serialization.LowLevelWriting
{
    public interface IJsonWriter
    {
        void WriteBeginOfObject();

        void WriteEndOfObject();

        void WriteBeginOfArray();

        void WriteEndOfArray();

        void EnsureCapacity(int numberOfRequiredBufferSlots);

        void WriteAscii(char asciiCharacter);

        void WriteString(ReadOnlySpan<char> @string);
    }
}