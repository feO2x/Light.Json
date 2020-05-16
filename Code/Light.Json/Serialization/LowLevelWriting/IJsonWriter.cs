namespace Light.Json.Serialization.LowLevelWriting
{
    public interface IJsonWriter
    {
        int CurrentIndex { get; }

        int EnsuredIndex { get; }

        void WriteBeginOfObject();

        void WriteEndOfObject();

        void WriteBeginOfArray();

        void WriteEndOfArray();

        void WriteKeyValueSeparator();

        void WriteValueSeparator();

        void EnsureCapacityFromCurrentIndex(int numberOfRequiredBufferSlots);

        void WriteAscii(char asciiCharacter);

        void WriteCharacter(char character);

        void WriteEscapedCharacter(char escapeCharacter);
    }
}