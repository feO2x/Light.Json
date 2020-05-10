using System;
using Light.GuardClauses;

namespace Light.Json.Serialization.LowLevelWriting
{
    public struct JsonUtf8Writer : IJsonWriter
    {
        private readonly IInMemoryBufferProvider<byte> _bufferProvider;
        private byte[] _buffer;
        private int _currentIndex;

        public JsonUtf8Writer(IInMemoryBufferProvider<byte> bufferProvider)
            : this(bufferProvider.MustNotBeNull(nameof(bufferProvider)).GetInitialBuffer(), bufferProvider) { }

        public JsonUtf8Writer(byte[] buffer, IInMemoryBufferProvider<byte> bufferProvider)
        {
            _buffer = buffer.MustNotBeNull(nameof(buffer));
            _bufferProvider = bufferProvider.MustNotBeNull(nameof(bufferProvider));
            _currentIndex = 0;
        }

        public void WriteBeginOfObject() => this.WriteSingleAsciiCharacter('{');

        public void WriteEndOfObject() => this.WriteSingleAsciiCharacter('}');

        public void WriteBeginOfArray() => this.WriteSingleAsciiCharacter('[');

        public void WriteEndOfArray() => this.WriteSingleAsciiCharacter(']');

        public void WriteString(ReadOnlySpan<char> @string)
        {
            WriteAscii('\"');
            for (var i = 0; i < @string.Length; i++)
            {
                var character = @string[i];
                switch (character)
                {
                    case '"':
                    case '\\':
                    case '\b':
                    case '\f':
                    case '\n':
                    case '\r':
                    case '\t':
                        WriteEscapedCharacter(character, @string.Length, i);
                        break;
                    default:
                        WriteAscii(character);
                        break;
                }
            }

            WriteAscii('\"');
        }

        public Memory<byte> ToUtf8Json() => new Memory<byte>(_buffer, 0, _currentIndex);

        private void WriteCharacter(byte character) => _buffer[_currentIndex++] = character;

        public void EnsureCapacity(int numberOfRequiredBufferSlots)
        {
            var requiredIndex = _currentIndex + numberOfRequiredBufferSlots;
            if (requiredIndex < _buffer.Length)
                return;

            _buffer = _bufferProvider.GetNewBufferWithIncreasedSize(_buffer, requiredIndex - _buffer.Length + 1);
        }

        public void WriteAscii(char asciiCharacter) => WriteCharacter((byte) asciiCharacter);

        private void WriteEscapedCharacter(char escapedCharacter, int stringLength, int currentIndex)
        {
            EnsureOneMoreInJsonString(stringLength, currentIndex);
            WriteAscii('\\');
            WriteAscii(escapedCharacter);
        }

        private void EnsureOneMoreInJsonString(int initialLength, int currentIndex) =>
            EnsureCapacity(initialLength - currentIndex + 2);
    }
}