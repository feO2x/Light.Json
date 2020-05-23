using System;
using Light.GuardClauses;
using Light.Json.Contracts;

namespace Light.Json.Serialization.LowLevelWriting
{
    public struct JsonUtf8Writer : IJsonWriter
    {
        private readonly IInMemoryBufferProvider<byte> _bufferProvider;
        private byte[] _buffer;

        public JsonUtf8Writer(IInMemoryBufferProvider<byte> bufferProvider)
        {
            _bufferProvider = bufferProvider.MustNotBeNull(nameof(bufferProvider));
            _buffer = bufferProvider.GetInitialBuffer();
            CurrentIndex = EnsuredIndex = 0;
        }

        public int CurrentIndex { get; private set; }

        public int EnsuredIndex { get; private set; }

        public void WriteBeginOfObject() => this.WriteSingleAsciiCharacter('{');

        public void WriteEndOfObject() => this.WriteSingleAsciiCharacter('}');

        public void WriteBeginOfArray() => this.WriteSingleAsciiCharacter('[');

        public void WriteEndOfArray() => this.WriteSingleAsciiCharacter(']');

        public void WriteKeyValueSeparator() => this.WriteSingleAsciiCharacter(':');

        public void WriteValueSeparator() => this.WriteSingleAsciiCharacter(',');

        public Memory<byte> ToUtf8Json() => new Memory<byte>(_buffer, 0, CurrentIndex);

        public byte[] Finish()
        {
            var utf8Json = ToUtf8Json().ToArray();
            _bufferProvider.Finish(_buffer);
            return utf8Json;
        }

        public void WriteCharacter(char character)
        {
            if (character < 128)
                WriteCharacter((byte) character);
            else if (character < 2048)
                WriteTwoByteCharacter(character);
            else
                WriteThreeByteCharacter(character);
        }

        private void WriteTwoByteCharacter(int character)
        {
            EnsureOneMoreAdditionalCapacity();
            // The first byte holds the upper 5 bits, prefixed with binary 110
            var firstByte = (byte) (0b1100_0000 | (character >> 6)); // The lower 6 bit are shifted out
            // The second byte holds the lower 6 bits, prefixed with binary 10
            var secondByte = (byte) (0b1000_0000 | (character & 0b0011_1111));
            WriteCharacter(firstByte);
            WriteCharacter(secondByte);
        }

        private void WriteThreeByteCharacter(int character)
        {
            EnsureAdditionalCapacity(2);
            // The first byte holds the upper 4 bits, prefixed with binary 1110
            var firstByte = (byte) (0b1110_0000 | (character >> 12)); // The lower 12 bits are shifted out
            var secondByte = (byte) (0b1000_0000 | ((character >> 6) & 0b0011_1111)); // Take bits 7 to 12 and put them in the second byte
            var thirdByte = (byte) (0b1000_0000 | (character & 0b0011_1111)); // Take the lowest six bits and put them in the third byte
            WriteCharacter(firstByte);
            WriteCharacter(secondByte);
            WriteCharacter(thirdByte);
        }

        private void WriteFourByteCharacter(int character)
        {
            EnsureAdditionalCapacity(3);
            var firstByte = (byte) (0b11110_000 | (character >> 18));
            var secondByte = (byte) (0b1000_0000 | ((character >> 12) & 0b0011_1111));
            var thirdByte = (byte) (0b1000_0000 | ((character >> 6) & 0b0011_1111));
            var fourthByte = (byte) (0b1000_0000 | (character & 0b0011_1111));
            WriteCharacter(firstByte);
            WriteCharacter(secondByte);
            WriteCharacter(thirdByte);
            WriteCharacter(fourthByte);
        }

        private void WriteCharacter(byte character) => _buffer[CurrentIndex++] = character;

        public void EnsureCapacityFromCurrentIndex(int numberOfAdditionalBufferSlots)
        {
            EnsuredIndex = CurrentIndex + numberOfAdditionalBufferSlots;
            EnsureCapacity();
        }

        private void EnsureCapacity()
        {
            if (EnsuredIndex < _buffer.Length)
                return;

            _buffer = _bufferProvider.GetNewBufferWithIncreasedSize(_buffer, EnsuredIndex - _buffer.Length + 1);
        }

        public void WriteAscii(char asciiCharacter) => WriteCharacter((byte) asciiCharacter);

        public void WriteSurrogatePair(char highSurrogate, char lowSurrogate)
        {
            var codePoint = char.ConvertToUtf32(highSurrogate, lowSurrogate);
            if (codePoint < 65536)
                WriteThreeByteCharacter(codePoint);
            else
                WriteFourByteCharacter(codePoint);
        }

        public void WriteContractConstantAsObjectKey(in ContractConstant constant)
        {
            var utf8Constant = constant.Utf8;
            EnsureCapacityFromCurrentIndex(utf8Constant.Length + 2);
            WriteAscii('\"');
            for (var i = 0; i < utf8Constant.Length; i++)
            {
                WriteCharacter(utf8Constant[i]);
            }
            WriteAscii('\"');
        }

        public void WriteEscapedCharacter(char escapedCharacter)
        {
            EnsureOneMoreAdditionalCapacity();
            WriteAscii('\\');
            WriteAscii(escapedCharacter);
        }

        private void EnsureOneMoreAdditionalCapacity()
        {
            ++EnsuredIndex;
            EnsureCapacity();
        }

        public void EnsureAdditionalCapacity(int numberOfAdditionalBufferSlots)
        {
            EnsuredIndex += numberOfAdditionalBufferSlots.MustBeGreaterThan(0, nameof(numberOfAdditionalBufferSlots));
            EnsureCapacity();
        }
    }
}