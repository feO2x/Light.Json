using System;
using Light.GuardClauses;
using Light.Json.FrameworkExtensions;

namespace Light.Json.Contracts
{
    public readonly struct ContractConstant : IEquatable<ContractConstant>
    {
        public readonly string Utf16;
        public readonly ReadOnlyMemory<byte> Utf8;
        public readonly bool ShouldFirstCharacterBeLowered;

        public ContractConstant(string utf16, bool shouldFirstCharacterBeLowered = true)
        {
            ShouldFirstCharacterBeLowered = shouldFirstCharacterBeLowered;
            utf16.MustNotBeNullOrWhiteSpace(nameof(utf16));
            Utf16 = shouldFirstCharacterBeLowered ? utf16.LowerFirstCharacter() : utf16;
            Utf8 = Utf16.ToUtf8();
        }

        public static implicit operator ContractConstant(string utf16) => new ContractConstant(utf16);

        public bool Equals(ContractConstant other) => other.Utf16 == Utf16;

        public override bool Equals(object? obj) =>
            obj is ContractConstant contractConstant && Equals(contractConstant);

        public override int GetHashCode() => Utf16?.GetHashCode() ?? 0;

        public override string ToString() => Utf16;
    }
}