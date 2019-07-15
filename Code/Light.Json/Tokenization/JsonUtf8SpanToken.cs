using System;
using System.Runtime.CompilerServices;
using Light.Json.FrameworkExtensions;

namespace Light.Json.Tokenization
{
    public readonly ref struct JsonUtf8SpanToken
    {
        public readonly JsonTokenType Type;
        public readonly ReadOnlySpan<byte> Text;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JsonUtf8SpanToken(JsonTokenType type, ReadOnlySpan<byte> text = default)
        {
            Type = type;
            Text = text;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(JsonUtf8SpanToken other) =>
            Type == other.Type && (Text == other.Text || Text.SequenceEqual(other.Text));

        public override bool Equals(object obj) =>
            throw BoxingNotSupported.CreateException();

        public override int GetHashCode() =>
            throw BoxingNotSupported.CreateException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(JsonUtf8SpanToken x, JsonUtf8SpanToken y) => x.Equals(y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(JsonUtf8SpanToken x, JsonUtf8SpanToken y) => !x.Equals(y);
    }
}