using System;
using Light.GuardClauses;

namespace Light.Json.Serialization.LowLevelWriting
{
    public struct JsonUtf16Writer : IJsonWriter
    {
        private char[] _buffer;
        private int _currentIndex;
        private readonly IInMemoryBufferProvider<char> _bufferProvider;

        public JsonUtf16Writer(IInMemoryBufferProvider<char> bufferProvider) :
            this(bufferProvider.MustNotBeNull(nameof(bufferProvider)).GetInitialBuffer(), bufferProvider) { }

        public JsonUtf16Writer(char[] buffer, IInMemoryBufferProvider<char> bufferProvider)
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
            EnsureCapacity(@string.Length + 2);

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

        public Memory<char> ToUtf16Json() => new Memory<char>(_buffer, 0, _currentIndex);

        public void WriteAscii(char asciiCharacter) => _buffer[_currentIndex++] = asciiCharacter;

        public void EnsureCapacity(int numberOfRequiredBufferSlots)
        {
            var requiredIndex = _currentIndex + numberOfRequiredBufferSlots;
            if (requiredIndex < _buffer.Length)
                return;

            _buffer = _bufferProvider.GetNewBufferWithIncreasedSize(_buffer, requiredIndex - _buffer.Length + 1);
        }

        public string Finish()
        {
            var json = ToUtf16Json().ToString();
            _bufferProvider.Finish(_buffer);
            return json;
        }

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