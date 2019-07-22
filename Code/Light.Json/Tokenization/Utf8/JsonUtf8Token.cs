using System;
using System.Runtime.CompilerServices;
using Light.Json.FrameworkExtensions;

namespace Light.Json.Tokenization.Utf8
{
    public readonly ref struct JsonUtf8Token
    {
        public readonly JsonTokenType Type;
        public readonly ReadOnlySpan<byte> Text;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JsonUtf8Token(JsonTokenType type, ReadOnlySpan<byte> text = default)
        {
            Type = type;
            Text = text;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(JsonUtf8Token other) =>
            Type == other.Type && (Text == other.Text || Text.SequenceEqual(other.Text));

        public override bool Equals(object obj) =>
            throw BoxingNotSupported.CreateException();

        public override int GetHashCode() =>
            throw BoxingNotSupported.CreateException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(JsonUtf8Token x, JsonUtf8Token y) => x.Equals(y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(JsonUtf8Token x, JsonUtf8Token y) => !x.Equals(y);
    }
}