using System;
using System.Runtime.CompilerServices;
using Light.GuardClauses;
using Light.Json.Buffers;

namespace Light.Json.Serialization.LowLevelWriting
{
    public struct JsonUtf16Writer : IJsonWriter
    {
        public JsonUtf16Writer(IBufferProvider<char> bufferProvider)
        {
            BufferProvider = bufferProvider.MustNotBeNull(nameof(bufferProvider));
            CurrentBuffer = bufferProvider.GetInitialBuffer();
            CurrentIndex = EnsuredIndex = 0;
        }

        public IBufferProvider<char> BufferProvider { get; }
        public char[] CurrentBuffer { get; private set; }

        public int CurrentIndex { get; private set; }

        public int EnsuredIndex { get; private set; }

        public bool IsCompatibleWithOptimizedContract => true;

        public void WriteBeginOfObject() => WriteSingleAsciiCharacter('{');

        public void WriteEndOfObject() => WriteSingleAsciiCharacter('}');

        public void WriteBeginOfArray() => WriteSingleAsciiCharacter('[');

        public void WriteEndOfArray() => WriteSingleAsciiCharacter(']');

        public void WriteKeyValueSeparator() => WriteSingleAsciiCharacter(':');

        public void WriteValueSeparator() => WriteSingleAsciiCharacter(',');

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteCharacter(char character) => CurrentBuffer[CurrentIndex++] = character;


        public void WriteAscii(char asciiCharacter) => WriteCharacter(asciiCharacter);

        public void WriteSurrogatePair(char highSurrogate, char lowSurrogate)
        {
            WriteCharacter(highSurrogate);
            WriteCharacter(lowSurrogate);
        }

        public void WriteConstantValueAsObjectKey(in ConstantValue constantValue)
        {
            var utf16Constant = constantValue.Utf16;
            EnsureCapacityFromCurrentIndex(utf16Constant.Length + 2);
            WriteAscii('\"');
            WriteRawCharacters(utf16Constant.AsSpan());
            WriteAscii('\"');
        }

        public void WriteConstantValue(in ConstantValue constantValue)
        {
            EnsureCapacityFromCurrentIndex(constantValue.Utf16.Length);
            WriteRawCharacters(constantValue.Utf16.AsSpan());
        }

        public void WriteConstantValue1(in ConstantValue constantValue)
        {
            EnsureCapacityFromCurrentIndex(1);
            WriteCharacter(constantValue.Utf16[0]);
        }

        public void WriteConstantValue2(in ConstantValue constantValue)
        {
            EnsureCapacityFromCurrentIndex(2);
            var utf16Constant = constantValue.Utf16;
            CurrentBuffer[CurrentIndex] = utf16Constant[0];
            CurrentBuffer[CurrentIndex + 1] = utf16Constant[1];
            CurrentIndex += 2;
        }

        public void WriteConstantValue3(in ConstantValue constantValue)
        {
            EnsureCapacityFromCurrentIndex(3);
            var utf16Constant = constantValue.Utf16;
            CurrentBuffer[CurrentIndex] = utf16Constant[0];
            CurrentBuffer[CurrentIndex + 1] = utf16Constant[1];
            CurrentBuffer[CurrentIndex + 2] = utf16Constant[2];
            CurrentIndex += 3;
        }

        public void WriteConstantValue4(in ConstantValue constantValue)
        {
            EnsureCapacityFromCurrentIndex(4);
            var utf16Constant = constantValue.Utf16;
            CurrentBuffer[CurrentIndex] = utf16Constant[0];
            CurrentBuffer[CurrentIndex + 1] = utf16Constant[1];
            CurrentBuffer[CurrentIndex + 2] = utf16Constant[2];
            CurrentBuffer[CurrentIndex + 3] = utf16Constant[3];
            CurrentIndex += 4;
        }

        public void WriteConstantValue5(in ConstantValue constantValue)
        {
            EnsureCapacityFromCurrentIndex(5);
            var utf16Constant = constantValue.Utf16;
            CurrentBuffer[CurrentIndex] = utf16Constant[0];
            CurrentBuffer[CurrentIndex + 1] = utf16Constant[1];
            CurrentBuffer[CurrentIndex + 2] = utf16Constant[2];
            CurrentBuffer[CurrentIndex + 3] = utf16Constant[3];
            CurrentBuffer[CurrentIndex + 4] = utf16Constant[4];
            CurrentIndex += 5;
        }

        public void WriteConstantValue6(in ConstantValue constantValue)
        {
            EnsureCapacityFromCurrentIndex(6);
            var utf16Constant = constantValue.Utf16;
            CurrentBuffer[CurrentIndex] = utf16Constant[0];
            CurrentBuffer[CurrentIndex + 1] = utf16Constant[1];
            CurrentBuffer[CurrentIndex + 2] = utf16Constant[2];
            CurrentBuffer[CurrentIndex + 3] = utf16Constant[3];
            CurrentBuffer[CurrentIndex + 4] = utf16Constant[4];
            CurrentBuffer[CurrentIndex + 5] = utf16Constant[5];
            CurrentIndex += 6;
        }

        public void WriteConstantValue7(in ConstantValue constantValue)
        {
            EnsureCapacityFromCurrentIndex(7);
            var utf16Constant = constantValue.Utf16;
            CurrentBuffer[CurrentIndex] = utf16Constant[0];
            CurrentBuffer[CurrentIndex + 1] = utf16Constant[1];
            CurrentBuffer[CurrentIndex + 2] = utf16Constant[2];
            CurrentBuffer[CurrentIndex + 3] = utf16Constant[3];
            CurrentBuffer[CurrentIndex + 4] = utf16Constant[4];
            CurrentBuffer[CurrentIndex + 5] = utf16Constant[5];
            CurrentBuffer[CurrentIndex + 6] = utf16Constant[6];
            CurrentIndex += 7;
        }

        public void WriteConstantValueLarge(in ConstantValue constantValue)
        {
            EnsureCapacityFromCurrentIndex(constantValue.Utf16.Length);
            CopyMemoryUnsafe(constantValue.Utf16.AsSpan());
        }

        public override string ToString() => ToUtf16JsonSpan().ToString();

        public Span<char> ToUtf16JsonSpan() => new Span<char>(CurrentBuffer, 0, CurrentIndex);

        public Memory<char> ToUtf16JsonMemory() => new Memory<char>(CurrentBuffer, 0, CurrentIndex);

        public Utf16SerializationResult GetResult() => new Utf16SerializationResult(ToUtf16JsonMemory(), CurrentBuffer, BufferProvider);

        public void EnsureCapacityFromCurrentIndex(int numberOfAdditionalBufferSlots)
        {
            EnsuredIndex = CurrentIndex + numberOfAdditionalBufferSlots;
            EnsureCapacity();
        }

        public void EnsureAdditionalCapacity(int numberOfAdditionalBufferSlots)
        {
            EnsuredIndex += numberOfAdditionalBufferSlots.MustBeGreaterThan(0, nameof(numberOfAdditionalBufferSlots));
            EnsureCapacity();
        }

        private void EnsureCapacity()
        {
            if (EnsuredIndex < CurrentBuffer.Length)
                return;

            CurrentBuffer = BufferProvider.GetNewBufferWithIncreasedSize(CurrentBuffer, EnsuredIndex - CurrentBuffer.Length + 1);
        }

        private void WriteSingleAsciiCharacter(char character)
        {
            EnsureCapacityFromCurrentIndex(1);
            WriteCharacter(character);
        }

        private void WriteRawCharacters(in ReadOnlySpan<char> characters)
        {
            switch (characters.Length)
            {
                case 0: return;
                case 1:
                    WriteCharacter(characters[0]);
                    return;
                case 2:
                    WriteCharacter(characters[0]);
                    WriteCharacter(characters[1]);
                    return;
                case 3:
                    WriteCharacter(characters[0]);
                    WriteCharacter(characters[1]);
                    WriteCharacter(characters[2]);
                    return;
                case 4:
                    WriteCharacter(characters[0]);
                    WriteCharacter(characters[1]);
                    WriteCharacter(characters[2]);
                    WriteCharacter(characters[3]);
                    return;
                case 5:
                    WriteCharacter(characters[0]);
                    WriteCharacter(characters[1]);
                    WriteCharacter(characters[2]);
                    WriteCharacter(characters[3]);
                    WriteCharacter(characters[4]);
                    return;
                case 6:
                    WriteCharacter(characters[0]);
                    WriteCharacter(characters[1]);
                    WriteCharacter(characters[2]);
                    WriteCharacter(characters[3]);
                    WriteCharacter(characters[4]);
                    WriteCharacter(characters[5]);
                    return;
                case 7:
                    WriteCharacter(characters[0]);
                    WriteCharacter(characters[1]);
                    WriteCharacter(characters[2]);
                    WriteCharacter(characters[3]);
                    WriteCharacter(characters[4]);
                    WriteCharacter(characters[5]);
                    WriteCharacter(characters[6]);
                    return;
                default:
                    CopyMemoryUnsafe(characters);
                    return;
            }
        }

        private unsafe void CopyMemoryUnsafe(in ReadOnlySpan<char> characters)
        {
            fixed (void* source = &characters[0], target = &CurrentBuffer[CurrentIndex])
                Buffer.MemoryCopy(source, target, Buffer.ByteLength(CurrentBuffer) - CurrentIndex * 2, characters.Length * 2);
            CurrentIndex += characters.Length;
        }
    }
}