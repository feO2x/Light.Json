using System;

namespace Light.Json.Serialization.LowLevelWriting
{
    public interface IJsonWriter
    {
        void WriteBeginOfObject();

        void WriteEndOfObject();

        void WriteBeginOfArray();

        void WriteEndOfArray();

        void WriteNameValueSeparator();

        void WriteEntrySeparator();

        void WriteTrue();

        void WriteFalse();

        void WriteNull();

        void WriteString(ReadOnlySpan<char> @string);
    }
}