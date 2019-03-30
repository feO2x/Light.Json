using System;
using System.Runtime.CompilerServices;
using Light.Json.FrameworkExtensions;

namespace Light.Json
{
    public readonly ref struct JsonTextSpanToken
    {
        public readonly JsonTokenType Type;
        public readonly ReadOnlySpan<char> Text;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JsonTextSpanToken(JsonTokenType type, ReadOnlySpan<char> text = default)
        {
            Type = type;
            Text = text;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(JsonTextSpanToken other) =>
            Type == other.Type && (Text == other.Text || Text.Equals(other.Text, StringComparison.Ordinal));

        public override bool Equals(object obj) =>
            throw BoxingNotSupported.CreateException();


        public override int GetHashCode() =>
            throw BoxingNotSupported.CreateException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(JsonTextSpanToken x, JsonTextSpanToken y) => x.Equals(y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(JsonTextSpanToken x, JsonTextSpanToken y) => !x.Equals(y);
    }
}