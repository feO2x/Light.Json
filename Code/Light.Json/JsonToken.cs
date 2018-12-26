using System;

namespace Light.Json
{
    public readonly ref struct JsonToken
    {
        public readonly JsonTokenType Type;
        public readonly ReadOnlySpan<char> Text;

        public JsonToken(JsonTokenType type, ReadOnlySpan<char> text = default)
        {
            Type = type;
            Text = text;
        }

        public bool Equals(JsonToken other) =>
            Type == other.Type &&
            Text == other.Text;

        public override bool Equals(object obj) =>
            throw new NotSupportedException("ref structs do not support object.Equals as they cannot live on the heap.");

        public override int GetHashCode() =>
            throw new NotSupportedException("ref structs do not support object.GetHashCode as they cannot live on the heap.");

        public static bool operator ==(JsonToken x, JsonToken y) => x.Equals(y);
        public static bool operator !=(JsonToken x, JsonToken y) => !x.Equals(y);
    }
}