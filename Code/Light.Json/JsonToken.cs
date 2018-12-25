using System;

namespace Light.Json
{
    public readonly ref struct JsonToken
    {
        public readonly JsonTokenType Type;
        public readonly ReadOnlySpan<char> Text;

        private JsonToken(JsonTokenType type, ReadOnlySpan<char> text)
        {
            Type = type;
            Text = text;
        }

        public static JsonToken IntegerNumber(ReadOnlySpan<char> numberText) => new JsonToken(JsonTokenType.IntegerNumber, numberText);

        public static JsonToken FloatingPointNumber(ReadOnlySpan<char> numberText) => new JsonToken(JsonTokenType.FloatingPointNumber, numberText);

        public static JsonToken String(ReadOnlySpan<char> text) => new JsonToken(JsonTokenType.String, text);
    }
}