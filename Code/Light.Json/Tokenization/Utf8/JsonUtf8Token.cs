using System;
using Light.GuardClauses.Exceptions;
using Light.Json.FrameworkExtensions;

namespace Light.Json.Tokenization.Utf8
{
    public readonly struct JsonUtf8Token : IJsonToken
    {
        public JsonUtf8Token(JsonTokenType type,
                             ReadOnlyMemory<byte> memory,
                             int length,
                             int charLength,
                             int line,
                             int position)
        {
            Type = type;
            Memory = memory;
            Length = length;
            CharLength = charLength;
            Line = line;
            Position = position;
        }

        public JsonTokenType Type { get; }

        public ReadOnlyMemory<byte> Memory { get; }

        public int Length { get; }

        public int CharLength { get; }

        public int Line { get; }

        public int Position { get; }

        public TokenCharacterInfo GetCharacterAt(int startIndex = 0)
        {
            var result = Utf8Character.TryParseNext(Memory.Span, out var character, startIndex);
            if (result != Utf8ParseResult.CharacterParsedSuccessfully)
                throw new InvalidStateException($"The UTF8 character at index {startIndex} is invalid. This should not happen - the tokenizer created an invalid token.");

            startIndex += character.ByteLength;
            if (startIndex == Memory.Length)
                startIndex = -1;

            return new TokenCharacterInfo(character.Utf16Code, startIndex);
        }

        public bool Equals(JsonUtf8Token other) =>
            Type == other.Type && Memory.Equals(other.Memory);

        public override string ToString()
        {
            Span<char> target = stackalloc char[CharLength];
            var currentIndex = 0;
            var span = Memory.Span;
            for (var i = 0; i < Length; ++i)
            {
                Utf8Character.TryParseNext(span, out var utf8Character, currentIndex);
                currentIndex += utf8Character.CopyUtf16To(target, currentIndex);
            }

            return target.ToString();
        }

        public override bool Equals(object obj) =>
            throw BoxingNotSupported.CreateException();

        public override int GetHashCode() =>
            throw BoxingNotSupported.CreateException();

        public static bool operator ==(JsonUtf8Token x, JsonUtf8Token y) => x.Equals(y);

        public static bool operator !=(JsonUtf8Token x, JsonUtf8Token y) => !x.Equals(y);
    }
}