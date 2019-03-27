using System;

namespace Light.Json
{
    public readonly ref struct JsonTextToken
    {
        public readonly JsonTokenType Type;
        public readonly ReadOnlySpan<char> Text;

        public JsonTextToken(JsonTokenType type, ReadOnlySpan<char> text = default)
        {
            Type = type;
            Text = text;
        }

        public bool Equals(JsonTextToken other) =>
            Type == other.Type &&
            (Text == other.Text ||
             Text.Equals(other.Text, StringComparison.Ordinal));

        public override bool Equals(object obj) =>
            throw new NotSupportedException("ref structs do not support object.Equals as they cannot live on the heap.");

        public override int GetHashCode() =>
            throw new NotSupportedException("ref structs do not support object.GetHashCode as they cannot live on the heap.");

        public static bool operator ==(JsonTextToken x, JsonTextToken y) => x.Equals(y);
        public static bool operator !=(JsonTextToken x, JsonTextToken y) => !x.Equals(y);
    }
}