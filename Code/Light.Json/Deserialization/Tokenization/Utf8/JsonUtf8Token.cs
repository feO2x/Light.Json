using System;
using Light.Json.Buffers;
using Light.Json.FrameworkExtensions;

namespace Light.Json.Deserialization.Tokenization.Utf8
{
    public readonly struct JsonUtf8Token : IJsonToken, IEquatable<JsonUtf8Token>
    {
        public JsonUtf8Token(JsonTokenType type,
                             ReadOnlyMemory<byte> memory,
                             int line,
                             int position)
        {
            Type = type;
            Memory = memory;
            Line = line;
            Position = position;
        }

        public JsonTokenType Type { get; }

        public ReadOnlyMemory<byte> Memory { get; }

        public int Line { get; }

        public int Position { get; }

        public bool Equals(in ConstantValue constant) =>
            Memory.Length <= 2 ? constant.Utf8.Length == 0 : Memory.Slice(1, Memory.Length - 2).Span.SequenceEqual(constant.Utf8);

        public bool Equals(JsonUtf8Token other) =>
            Type == other.Type && Memory.Equals(other.Memory) && Line == other.Line && Position == other.Position;


        public override bool Equals(object? obj) =>
            obj is JsonUtf8Token token && Equals(token);

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

        public override string ToString() => Memory.Span.ConvertFromUtf8ToString();

        public static bool operator ==(JsonUtf8Token x, JsonUtf8Token y) => x.Equals(y);

        public static bool operator !=(JsonUtf8Token x, JsonUtf8Token y) => !x.Equals(y);
    }
}