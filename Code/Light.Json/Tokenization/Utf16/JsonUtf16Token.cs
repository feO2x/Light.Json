using System;
using Light.Json.FrameworkExtensions;

namespace Light.Json.Tokenization.Utf16
{
    public readonly struct JsonUtf16Token : IJsonToken
    {
        public JsonUtf16Token(JsonTokenType type, ReadOnlyMemory<char> memory, int line, int position)
        {
            Type = type;
            Memory = memory;
            Line = line;
            Position = position;
        }

        public JsonTokenType Type { get; }
        public ReadOnlyMemory<char> Memory { get; }
        public int Length => Memory.Length;
        public int Line { get; }
        public int Position { get; }

        public TokenCharacterInfo GetCharacterAt(int startIndex = 0)
        {
            var character = Memory.Span[startIndex++];
            if (startIndex == Memory.Length)
                startIndex = -1;
            return new TokenCharacterInfo(character, startIndex);
        }

        public string ParseJsonStringToDotnetString()
        {
            this.MustBeOfType(JsonTokenType.String);
            return Memory.Slice(1, Memory.Length - 2).ToString();
        }

        public bool Equals(JsonUtf16Token other) => Type == other.Type && Memory.Equals(other.Memory);

        public override bool Equals(object obj) => throw BoxingNotSupported.CreateException();

        public override int GetHashCode() => throw BoxingNotSupported.CreateException();

        public static bool operator ==(JsonUtf16Token x, JsonUtf16Token y) => x.Equals(y);

        public static bool operator !=(JsonUtf16Token x, JsonUtf16Token y) => !x.Equals(y);
    }
}