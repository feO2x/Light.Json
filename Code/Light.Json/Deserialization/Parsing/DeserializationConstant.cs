using System;
using Light.GuardClauses;
using Light.Json.FrameworkExtensions;

namespace Light.Json.Deserialization.Parsing
{
    public readonly struct DeserializationConstant
    {
        public readonly string Utf16;
        public readonly ReadOnlyMemory<byte> Utf8;

        public DeserializationConstant(string utf16)
        {
            Utf16 = utf16.MustNotBeNullOrWhiteSpace(nameof(utf16));
            Utf8 = utf16.ToUtf8();
        }

        public static implicit operator DeserializationConstant(string utf16) => new DeserializationConstant(utf16);
    }
}