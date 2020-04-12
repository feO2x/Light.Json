using System;
using Light.Json.Deserialization.Parsing;

namespace Light.Json.Deserialization.Tokenization.Utf16
{
    public readonly struct JsonUtf16Token : IJsonToken, IEquatable<JsonUtf16Token>
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

        public int Line { get; }

        public int Position { get; }

        public bool Equals(in DeserializationConstant constant) =>
            Memory.Length <= 2 ? constant.Utf16 == string.Empty : Memory.Slice(1, Memory.Length - 2).Span.Equals(constant.Utf16.AsSpan(), StringComparison.Ordinal);

        public bool Equals(JsonUtf16Token other) =>
            Type == other.Type && Memory.Equals(other.Memory) && Line == other.Line && Position == other.Position;

        public override bool Equals(object? obj) =>
            obj is JsonUtf16Token other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) Type;
                hashCode = (hashCode * 397) ^ Memory.GetHashCode();
                hashCode = (hashCode * 397) ^ Line;
                hashCode = (hashCode * 397) ^ Position;
                return hashCode;
            }
        }

        public static bool operator ==(JsonUtf16Token x, JsonUtf16Token y) => x.Equals(y);

        public static bool operator !=(JsonUtf16Token x, JsonUtf16Token y) => !x.Equals(y);

        public override string ToString() => Memory.ToString();
    }
}