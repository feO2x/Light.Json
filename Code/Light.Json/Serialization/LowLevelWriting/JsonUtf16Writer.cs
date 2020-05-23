using System;
using Light.GuardClauses;
using Light.Json.Contracts;

namespace Light.Json.Serialization.LowLevelWriting
{
    public struct JsonUtf16Writer : IJsonWriter
    {
        private char[] _buffer;
        private readonly IInMemoryBufferProvider<char> _bufferProvider;

        public JsonUtf16Writer(IInMemoryBufferProvider<char> bufferProvider)
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

        public Memory<char> ToUtf16Json() => new Memory<char>(_buffer, 0, CurrentIndex);

        public void WriteCharacter(char character) => _buffer[CurrentIndex++] = character;

        public void WriteAscii(char asciiCharacter) => WriteCharacter(asciiCharacter);

        public void WriteSurrogatePair(char highSurrogate, char lowSurrogate)
        {
            WriteCharacter(highSurrogate);
            WriteCharacter(lowSurrogate);
        }

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

        public string Finish()
        {
            var json = ToUtf16Json().ToString();
            _bufferProvider.Finish(_buffer);
            return json;
        }

        public void WriteEscapedCharacter(char escapedCharacter)
        {
            EnsureOneMore();
            WriteAscii('\\');
            WriteAscii(escapedCharacter);
        }

        public void WriteContractConstantAsObjectKey(in ContractConstant constant)
        {
            var utf16Constant = constant.Utf16;
            EnsureCapacityFromCurrentIndex(utf16Constant.Length + 2);
            WriteAscii('\"');
            utf16Constant.CopyTo(0, _buffer, CurrentIndex, utf16Constant.Length);
            CurrentIndex += utf16Constant.Length;
            WriteAscii('\"');
        }

        private void EnsureOneMore()
        {
            ++EnsuredIndex;
            EnsureCapacity();
        }

        private void EnsureCapacity()
        {
            if (EnsuredIndex < _buffer.Length)
                return;

            _buffer = _bufferProvider.GetNewBufferWithIncreasedSize(_buffer, EnsuredIndex - _buffer.Length + 1);
        }
    }
}