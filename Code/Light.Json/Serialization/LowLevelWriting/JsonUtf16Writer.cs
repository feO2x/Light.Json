using System;
using Light.GuardClauses;
using Light.Json.Serialization.Buffers;

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

        public void WriteBeginOfObject() => WriteSingleCharacter('{');

        public void WriteEndOfObject() => WriteSingleCharacter('}');

        public void WriteBeginOfArray() => WriteSingleCharacter('[');

        public void WriteEndOfArray() => WriteSingleCharacter(']');

        public void WriteNameValueSeparator() => WriteSingleCharacter(':');

        public void WriteEntrySeparator() => WriteSingleCharacter(',');

        public void WriteTrue()
        {
            EnsureCapacity(4);
            WriteCharacter('t');
            WriteCharacter('r');
            WriteCharacter('u');
            WriteCharacter('e');
        }

        public void WriteFalse()
        {
            EnsureCapacity(5);
            WriteCharacter('f');
            WriteCharacter('a');
            WriteCharacter('l');
            WriteCharacter('s');
            WriteCharacter('e');
        }

        public void WriteNull()
        {
            EnsureCapacity(4);
            WriteCharacter('n');
            WriteCharacter('u');
            WriteCharacter('l');
            WriteCharacter('l');
        }

        public void WriteString(ReadOnlySpan<char> @string)
        {
            EnsureCapacity(@string.Length + 2);

            WriteCharacter('\"');
            for (var i = 0; i < @string.Length; i++)
            {
                var character = @string[i];
                switch (character)
                {
                    case '"':
                        EnsureOneMoreInJsonString(@string.Length, i);
                        WriteCharacter('\\');
                        WriteCharacter('"');
                        break;
                    case '\\':
                        EnsureOneMoreInJsonString(@string.Length, i);
                        WriteCharacter('\\');
                        WriteCharacter('\\');
                        break;
                    case '\b':
                        EnsureOneMoreInJsonString(@string.Length, i);
                        WriteCharacter('\\');
                        WriteCharacter('b');
                        break;
                    case '\f':
                        EnsureOneMoreInJsonString(@string.Length, i);
                        WriteCharacter('\\');
                        WriteCharacter('f');
                        break;
                    case '\n':
                        EnsureOneMoreInJsonString(@string.Length, i);
                        WriteCharacter('\\');
                        WriteCharacter('n');
                        break;
                    case '\r':
                        EnsureOneMoreInJsonString(@string.Length, i);
                        WriteCharacter('\\');
                        WriteCharacter('r');
                        break;
                    case '\t':
                        EnsureOneMoreInJsonString(@string.Length, i);
                        WriteCharacter('\\');
                        WriteCharacter('t');
                        break;
                    default:
                        WriteCharacter(character);
                        break;
                }
            }

            WriteCharacter('\"');
        }

        public Memory<char> ToUtf16Json() => new Memory<char>(_buffer, 0, _currentIndex);

        public void WriteSingleCharacter(char character)
        {
            EnsureCapacity(1);
            WriteCharacter(character);
        }

        public void WriteCharacter(char character) => _buffer[_currentIndex++] = character;

        public void EnsureCapacity(int numberOfRequiredBufferSlots)
        {
            var requiredIndex = _currentIndex + numberOfRequiredBufferSlots;
            if (requiredIndex < _buffer.Length)
                return;

            _buffer = _bufferProvider.GetNewBufferWithIncreasedSize(_buffer, requiredIndex - _buffer.Length + 1);
        }

        public void EnsureOneMoreInJsonString(int initialLength, int currentIndex) =>
            EnsureCapacity(initialLength - currentIndex + 2);
    }
}