using System;
using System.Runtime.CompilerServices;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;
using Light.Json.Buffers;

namespace Light.Json.Serialization.LowLevelWriting
{
    public struct JsonUtf8Writer : IJsonWriter
    {
        public JsonUtf8Writer(IBufferProvider<byte> bufferProvider)
        {
            BufferProvider = bufferProvider.MustNotBeNull(nameof(bufferProvider));
            CurrentBuffer = bufferProvider.GetInitialBuffer();
            CurrentIndex = EnsuredIndex = 0;
        }

        public byte[] CurrentBuffer { get; private set; }

        public IBufferProvider<byte> BufferProvider { get; }

        public int CurrentIndex { get; private set; }

        public int EnsuredIndex { get; private set; }

        public bool IsCompatibleWithOptimizedContract => true;

        public void WriteBeginOfObject() => WriteSingleAsciiCharacter('{');

        public void WriteEndOfObject() => WriteSingleAsciiCharacter('}');

        public void WriteBeginOfArray() => WriteSingleAsciiCharacter('[');

        public void WriteEndOfArray() => WriteSingleAsciiCharacter(']');

        public void WriteKeyValueSeparator() => WriteSingleAsciiCharacter(':');

        public void WriteValueSeparator() => WriteSingleAsciiCharacter(',');

        public override string ToString() => ToUtf8JsonSpan().ToString();

        public Memory<byte> ToUtf8JsonMemory() => new Memory<byte>(CurrentBuffer, 0, CurrentIndex);

        public Span<byte> ToUtf8JsonSpan() => new Span<byte>(CurrentBuffer, 0, CurrentIndex);

        public Utf8SerializationResult GetResult() => new Utf8SerializationResult(ToUtf8JsonMemory(), CurrentBuffer, BufferProvider);

        public void WriteCharacter(char character)
        {
            if (character < 128)
                WriteByte((byte) character);
            else if (character < 2048)
                WriteTwoByteCharacter(character);
            else
                WriteThreeByteCharacter(character);
        }

        public void WriteAscii(char asciiCharacter) => WriteByte((byte) asciiCharacter);

        public void WriteSurrogatePair(char highSurrogate, char lowSurrogate)
        {
            var codePoint = char.ConvertToUtf32(highSurrogate, lowSurrogate);
            if (codePoint < 65536)
                WriteThreeByteCharacter(codePoint);
            else
                WriteFourByteCharacter(codePoint);
        }

        public void WriteConstantValueAsObjectKey(in ConstantValue constantValue)
        {
            var utf8Constant = constantValue.Utf8;
            EnsureCapacityFromCurrentIndex(utf8Constant.Length + 2);
            WriteAscii('\"');
            WriteRawBytes(utf8Constant);
            WriteAscii('\"');
        }

        public void WriteConstantValue(in ConstantValue constant)
        {
            EnsureCapacityFromCurrentIndex(constant.Utf8.Length);
            WriteRawBytes(constant.Utf8);
        }

        public void WriteConstantValue1(in ConstantValue constantValue)
        {
            EnsureCapacityFromCurrentIndex(1);
            WriteByte(constantValue.Utf8[0]);
        }

        public void WriteConstantValue2(in ConstantValue constantValue)
        {
            EnsureCapacityFromCurrentIndex(2);
            var utf8Constant = constantValue.Utf8;
            CurrentBuffer[CurrentIndex] = utf8Constant[0];
            CurrentBuffer[CurrentIndex + 1] = utf8Constant[1];
            CurrentIndex += 2;
        }

        public void WriteConstantValue3(in ConstantValue constantValue)
        {
            EnsureCapacityFromCurrentIndex(3);
            var utf8Constant = constantValue.Utf8;
            CurrentBuffer[CurrentIndex] = utf8Constant[0];
            CurrentBuffer[CurrentIndex + 1] = utf8Constant[1];
            CurrentBuffer[CurrentIndex + 2] = utf8Constant[2];
            CurrentIndex += 3;
        }

        public void WriteConstantValue4(in ConstantValue constantValue)
        {
            EnsureCapacityFromCurrentIndex(4);
            var utf8Constant = constantValue.Utf8;
            CurrentBuffer[CurrentIndex] = utf8Constant[0];
            CurrentBuffer[CurrentIndex + 1] = utf8Constant[1];
            CurrentBuffer[CurrentIndex + 2] = utf8Constant[2];
            CurrentBuffer[CurrentIndex + 3] = utf8Constant[3];
            CurrentIndex += 4;
        }

        public void WriteConstantValue5(in ConstantValue constantValue)
        {
            EnsureCapacityFromCurrentIndex(5);
            var utf8Constant = constantValue.Utf8;
            CurrentBuffer[CurrentIndex] = utf8Constant[0];
            CurrentBuffer[CurrentIndex + 1] = utf8Constant[1];
            CurrentBuffer[CurrentIndex + 2] = utf8Constant[2];
            CurrentBuffer[CurrentIndex + 3] = utf8Constant[3];
            CurrentBuffer[CurrentIndex + 4] = utf8Constant[4];
            CurrentIndex += 5;
        }

        public void WriteConstantValue6(in ConstantValue constantValue)
        {
            EnsureCapacityFromCurrentIndex(6);
            var utf8Constant = constantValue.Utf8;
            CurrentBuffer[CurrentIndex] = utf8Constant[0];
            CurrentBuffer[CurrentIndex + 1] = utf8Constant[1];
            CurrentBuffer[CurrentIndex + 2] = utf8Constant[2];
            CurrentBuffer[CurrentIndex + 3] = utf8Constant[3];
            CurrentBuffer[CurrentIndex + 4] = utf8Constant[4];
            CurrentBuffer[CurrentIndex + 5] = utf8Constant[5];
            CurrentIndex += 6;
        }

        public void WriteConstantValue7(in ConstantValue constantValue)
        {
            EnsureCapacityFromCurrentIndex(7);
            var utf8Constant = constantValue.Utf8;
            CurrentBuffer[CurrentIndex] = utf8Constant[0];
            CurrentBuffer[CurrentIndex + 1] = utf8Constant[1];
            CurrentBuffer[CurrentIndex + 2] = utf8Constant[2];
            CurrentBuffer[CurrentIndex + 3] = utf8Constant[3];
            CurrentBuffer[CurrentIndex + 4] = utf8Constant[4];
            CurrentBuffer[CurrentIndex + 5] = utf8Constant[5];
            CurrentBuffer[CurrentIndex + 6] = utf8Constant[6];
            CurrentIndex += 7;
        }

        public void WriteConstantValueLarge(in ConstantValue constantValue)
        {
            EnsureCapacityFromCurrentIndex(constantValue.Utf8.Length);
            CopyMemoryUnsafe(constantValue.Utf8);
        }

        public void EnsureCapacityFromCurrentIndex(int numberOfAdditionalBufferSlots)
        {
            if (numberOfAdditionalBufferSlots < 1)
                Throw.MustBeGreaterThan(numberOfAdditionalBufferSlots, 0, nameof(numberOfAdditionalBufferSlots));

            EnsuredIndex = CurrentIndex + numberOfAdditionalBufferSlots;
            EnsureCapacity();
        }

        public void EnsureAdditionalCapacity(int numberOfAdditionalBufferSlots)
        {
            if (numberOfAdditionalBufferSlots < 1)
                Throw.MustBeGreaterThan(numberOfAdditionalBufferSlots, 0, nameof(numberOfAdditionalBufferSlots));

            EnsuredIndex += numberOfAdditionalBufferSlots;
            EnsureCapacity();
        }

        private void EnsureOneMoreAdditionalCapacity()
        {
            ++EnsuredIndex;
            EnsureCapacity();
        }

        private void EnsureCapacity()
        {
            if (EnsuredIndex < CurrentBuffer.Length)
                return;

            CurrentBuffer = BufferProvider.GetNewBufferWithIncreasedSize(CurrentBuffer, EnsuredIndex - CurrentBuffer.Length + 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteByte(byte character) => CurrentBuffer[CurrentIndex++] = character;

        private void WriteTwoByteCharacter(int character)
        {
            EnsureOneMoreAdditionalCapacity();
            // The first byte holds the upper 5 bits, prefixed with binary 110
            var firstByte = (byte) (0b1100_0000 | (character >> 6)); // The lower 6 bit are shifted out
            // The second byte holds the lower 6 bits, prefixed with binary 10
            var secondByte = (byte) (0b1000_0000 | (character & 0b0011_1111));
            WriteByte(firstByte);
            WriteByte(secondByte);
        }

        private void WriteThreeByteCharacter(int character)
        {
            EnsureAdditionalCapacity(2);
            // The first byte holds the upper 4 bits, prefixed with binary 1110
            var firstByte = (byte) (0b1110_0000 | (character >> 12)); // The lower 12 bits are shifted out
            var secondByte = (byte) (0b1000_0000 | ((character >> 6) & 0b0011_1111)); // Take bits 7 to 12 and put them in the second byte
            var thirdByte = (byte) (0b1000_0000 | (character & 0b0011_1111)); // Take the lowest six bits and put them in the third byte
            WriteByte(firstByte);
            WriteByte(secondByte);
            WriteByte(thirdByte);
        }

        private void WriteFourByteCharacter(int character)
        {
            EnsureAdditionalCapacity(3);
            var firstByte = (byte) (0b11110_000 | (character >> 18));
            var secondByte = (byte) (0b1000_0000 | ((character >> 12) & 0b0011_1111));
            var thirdByte = (byte) (0b1000_0000 | ((character >> 6) & 0b0011_1111));
            var fourthByte = (byte) (0b1000_0000 | (character & 0b0011_1111));
            WriteByte(firstByte);
            WriteByte(secondByte);
            WriteByte(thirdByte);
            WriteByte(fourthByte);
        }

        private void WriteSingleAsciiCharacter(char character)
        {
            EnsureCapacityFromCurrentIndex(1);
            WriteByte((byte) character);
        }

        private void WriteRawBytes(byte[] bytes)
        {
            switch (bytes.Length)
            {
                case 0: return;
                case 1:
                    WriteByte(bytes[0]);
                    return;
                case 2:
                    WriteByte(bytes[0]);
                    WriteByte(bytes[1]);
                    return;
                case 3:
                    WriteByte(bytes[0]);
                    WriteByte(bytes[1]);
                    WriteByte(bytes[2]);
                    return;
                case 4:
                    WriteByte(bytes[0]);
                    WriteByte(bytes[1]);
                    WriteByte(bytes[2]);
                    WriteByte(bytes[3]);
                    return;
                case 5:
                    WriteByte(bytes[0]);
                    WriteByte(bytes[1]);
                    WriteByte(bytes[2]);
                    WriteByte(bytes[3]);
                    WriteByte(bytes[4]);
                    return;
                case 6:
                    WriteByte(bytes[0]);
                    WriteByte(bytes[1]);
                    WriteByte(bytes[2]);
                    WriteByte(bytes[3]);
                    WriteByte(bytes[4]);
                    WriteByte(bytes[5]);
                    return;
                case 7:
                    WriteByte(bytes[0]);
                    WriteByte(bytes[1]);
                    WriteByte(bytes[2]);
                    WriteByte(bytes[3]);
                    WriteByte(bytes[4]);
                    WriteByte(bytes[5]);
                    WriteByte(bytes[6]);
                    return;
                default:
                    CopyMemoryUnsafe(bytes);
                    return;
            }
        }

        private unsafe void CopyMemoryUnsafe(byte[] bytes)
        {
            fixed (void* source = &bytes[0], target = &CurrentBuffer[CurrentIndex])
                Buffer.MemoryCopy(source, target, CurrentBuffer.Length - CurrentIndex, bytes.Length);
            CurrentIndex += bytes.Length;
        }
    }
}