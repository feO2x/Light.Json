using System;
using Light.GuardClauses;
using Light.Json.FrameworkExtensions;

namespace Light.Json.Buffers
{
    public readonly struct ConstantValue : IEquatable<ConstantValue>
    {
        public ConstantValue(string value, bool shouldFirstCharacterBeLowered)
        {
            value.MustNotBeNullOrWhiteSpace(nameof(value));
            if (shouldFirstCharacterBeLowered)
                value = value.LowerFirstCharacter();
            Utf16 = value;
            Utf8 = value.ToUtf8();
        }

        public string Utf16 { get; }
        public byte[] Utf8 { get; }

        public override string ToString() => Utf16;

        public bool Equals(ConstantValue other) => Utf16 == other.Utf16;

        public override bool Equals(object? obj) =>
            obj is ConstantValue other && Equals(other);

        public override int GetHashCode() => Utf16.GetHashCode();

        public static implicit operator ConstantValue(string value) => new ConstantValue(value, true);

        public static ConstantValue Unaltered(string value) => new ConstantValue(value, false);
    }
}