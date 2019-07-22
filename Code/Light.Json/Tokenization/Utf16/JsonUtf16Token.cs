using System;
using System.Runtime.CompilerServices;
using Light.Json.FrameworkExtensions;

namespace Light.Json.Tokenization.Utf16
{
    public readonly ref struct JsonUtf16Token
    {
        public readonly JsonTokenType Type;
        public readonly ReadOnlySpan<char> Text;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JsonUtf16Token(JsonTokenType type, ReadOnlySpan<char> text = default)
        {
            Type = type;
            Text = text;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(JsonUtf16Token other) =>
            Type == other.Type && (Text == other.Text || Text.Equals(other.Text, StringComparison.Ordinal));

        public override bool Equals(object obj) =>
            throw BoxingNotSupported.CreateException();


        public override int GetHashCode() =>
            throw BoxingNotSupported.CreateException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(JsonUtf16Token x, JsonUtf16Token y) => x.Equals(y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(JsonUtf16Token x, JsonUtf16Token y) => !x.Equals(y);
    }
}