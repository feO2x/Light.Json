using Light.Json.Buffers;

namespace Light.Json.Serialization.LowLevelWriting
{
    public interface IJsonWriter
    {
        int CurrentIndex { get; }

        int EnsuredIndex { get; }

        bool IsCompatibleWithOptimizedContract { get; }

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

        void WriteSurrogatePair(char highSurrogate, char lowSurrogate);

        void WriteConstantValueAsObjectKey(in ConstantValue constantValue);

        void WriteConstantValue(in ConstantValue constantValue);

        void WriteConstantValue1(in ConstantValue constantValue);
        void WriteConstantValue2(in ConstantValue constantValue);
        void WriteConstantValue3(in ConstantValue constantValue);
        void WriteConstantValue4(in ConstantValue constantValue);
        void WriteConstantValue5(in ConstantValue constantValue);
        void WriteConstantValue6(in ConstantValue constantValue);
        void WriteConstantValue7(in ConstantValue constantValue);
        void WriteConstantValueLarge(in ConstantValue constantValue);
    }
}