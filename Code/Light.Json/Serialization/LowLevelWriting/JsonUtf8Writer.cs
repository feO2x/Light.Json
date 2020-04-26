using System;

namespace Light.Json.Serialization.LowLevelWriting
{
    public struct JsonUtf8Writer : IJsonWriter
    {
        private readonly Memory<byte> _memory;
        private int _currentIndex;

        public JsonUtf8Writer(Memory<byte> memory)
        {
            _memory = memory;
            _currentIndex = 0;
        }

        public void WriteBeginOfObject() => WriteSingleCharacter((byte) '{');

        public void WriteEndOfObject() => WriteSingleCharacter((byte) '}');

        public void WriteBeginOfArray() => WriteSingleCharacter((byte) '[');

        public void WriteEndOfArray() => WriteSingleCharacter((byte) ']');

        public void WriteNameValueSeparator() => WriteSingleCharacter((byte) ':');

        public void WriteEntrySeparator() => WriteSingleCharacter((byte) ',');

        public void WriteTrue()
        {
            var span = _memory.Span;
            WriteSingleCharacter((byte) 't', in span);
            WriteSingleCharacter((byte) 'r', in span);
            WriteSingleCharacter((byte) 'u', in span);
            WriteSingleCharacter((byte) 'e', in span);
        }

        public void WriteFalse()
        {
            var span = _memory.Span;
            WriteSingleCharacter((byte) 'f', in span);
            WriteSingleCharacter((byte) 'a', in span);
            WriteSingleCharacter((byte) 'l', in span);
            WriteSingleCharacter((byte) 's', in span);
            WriteSingleCharacter((byte) 'e', in span);
        }

        public void WriteNull()
        {
            var span = _memory.Span;
            WriteSingleCharacter((byte) 'n', in span);
            WriteSingleCharacter((byte) 'u', in span);
            WriteSingleCharacter((byte) 'l', in span);
            WriteSingleCharacter((byte) 'l', in span);
        }

        public void WriteString(ReadOnlySpan<char> @string)
        {
            var span = _memory.Span;
            WriteSingleCharacter((byte) '\"', in span);
            for (var i = 0; i < @string.Length; i++)
            {
                var character = @string[i];
                switch (character)
                {
                    case '"':
                        WriteSingleCharacter((byte) '\\', in span);
                        WriteSingleCharacter((byte) '"', in span);
                        break;
                    case '\\':
                        WriteSingleCharacter((byte) '\\', in span);
                        WriteSingleCharacter((byte) '\\', in span);
                        break;
                    case '\b':
                        WriteSingleCharacter((byte) '\\', in span);
                        WriteSingleCharacter((byte) 'b', in span);
                        break;
                    case '\f':
                        WriteSingleCharacter((byte) '\\', in span);
                        WriteSingleCharacter((byte) 'f', in span);
                        break;
                    case '\n':
                        WriteSingleCharacter((byte) '\\', in span);
                        WriteSingleCharacter((byte) 'n', in span);
                        break;
                    case '\r':
                        WriteSingleCharacter((byte) '\\', in span);
                        WriteSingleCharacter((byte) 'r', in span);
                        break;
                    case '\t':
                        WriteSingleCharacter((byte) '\\', in span);
                        WriteSingleCharacter((byte) 't', in span);
                        break;
                    default:
                        WriteSingleCharacter((byte) character, in span);
                        break;
                }
            }
            WriteSingleCharacter((byte) '\"', in span);
        }

        public Memory<byte> ToUtf8Json() => _memory.Slice(0, _currentIndex);

        private void WriteSingleCharacter(byte character) => _memory.Span[_currentIndex++] = character;
        private void WriteSingleCharacter(byte character, in Span<byte> span) => span[_currentIndex++] = character;
    }
}