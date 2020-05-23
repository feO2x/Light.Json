using Light.Json.Contracts;

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

        void EnsureCapacityFromCurrentIndex(int numberOfAdditionalBufferSlots);

        void EnsureAdditionalCapacity(int numberOfAdditionalBufferSlots);

        void WriteAscii(char asciiCharacter);

        void WriteCharacter(char character);

        void WriteEscapedCharacter(char escapeCharacter);

        void WriteSurrogatePair(char highSurrogate, char lowSurrogate);

        void WriteContractConstantAsObjectKey(in ContractConstant constant);
    }
}