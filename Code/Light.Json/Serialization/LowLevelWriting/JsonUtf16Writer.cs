using System;

namespace Light.Json.Serialization.LowLevelWriting
{
    public struct JsonUtf16Writer : IJsonWriter
    {
        private readonly Memory<char> _memory;
        private int _currentIndex;

        public JsonUtf16Writer(Memory<char> memory)
        {
            _memory = memory;
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
            var span = _memory.Span;
            WriteSingleCharacter('t', in span);
            WriteSingleCharacter('r', in span);
            WriteSingleCharacter('u', in span);
            WriteSingleCharacter('e', in span);
        }

        public void WriteFalse()
        {
            var span = _memory.Span;
            WriteSingleCharacter('f', in span);
            WriteSingleCharacter('a', in span);
            WriteSingleCharacter('l', in span);
            WriteSingleCharacter('s', in span);
            WriteSingleCharacter('e', in span);
        }

        public void WriteNull()
        {
            var span = _memory.Span;
            WriteSingleCharacter('n', in span);
            WriteSingleCharacter('u', in span);
            WriteSingleCharacter('l', in span);
            WriteSingleCharacter('l', in span);
        }

        public void WriteString(ReadOnlySpan<char> @string)
        {
            var span = _memory.Span;
            WriteSingleCharacter('\"', span);
            for (var i = 0; i < @string.Length; i++)
            {
                var character = @string[i];
                switch (character)
                {
                    case '"':
                        WriteSingleCharacter('\\', in span);
                        WriteSingleCharacter('"', in span);
                        break;
                    case '\\':
                        WriteSingleCharacter('\\', in span);
                        WriteSingleCharacter('\\', in span);
                        break;
                    case '\b':
                        WriteSingleCharacter('\\', in span);
                        WriteSingleCharacter('b', in span);
                        break;
                    case '\f':
                        WriteSingleCharacter('\\', in span);
                        WriteSingleCharacter('f', in span);
                        break;
                    case '\n':
                        WriteSingleCharacter('\\', in span);
                        WriteSingleCharacter('n', in span);
                        break;
                    case '\r':
                        WriteSingleCharacter('\\', in span);
                        WriteSingleCharacter('r', in span);
                        break;
                    case '\t':
                        WriteSingleCharacter('\\', in span);
                        WriteSingleCharacter('t', in span);
                        break;
                    default:
                        WriteSingleCharacter(character, in span);
                        break;
                }
            }
            WriteSingleCharacter('\"', span);
        }

        public Memory<char> ToUtf16Json() => _memory.Slice(0, _currentIndex);

        private void WriteSingleCharacter(char character) => _memory.Span[_currentIndex++] = character;
        private void WriteSingleCharacter(char character, in Span<char> span) => span[_currentIndex++] = character;
    }
}