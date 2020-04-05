using System;
using Light.GuardClauses;

namespace Light.Json.FrameworkExtensions
{
    public readonly struct TypeKey : IEquatable<TypeKey>
    {
        public TypeKey(Type type, string? key = null)
        {
            Type = type.MustNotBeNull(nameof(Type));
            Key = key;
        }

        public Type Type { get; }

        public string? Key { get; }

        public bool Equals(TypeKey other) =>
            Type == other.Type && Key == other.Key;

        public override bool Equals(object? obj) =>
            obj is TypeKey other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (Type.GetHashCode() * 397) ^ (Key != null ? Key.GetHashCode() : 0);
            }
        }

        public static bool operator ==(TypeKey x, TypeKey y) => x.Equals(y);

        public static bool operator !=(TypeKey x, TypeKey y) => !x.Equals(y);

        public static implicit operator TypeKey(Type type) => new TypeKey(type);

        public override string ToString()
        {
            if (Type == null)
                return "Invalid Type Key";
            if (Key == null)
                return Type.Name;
            return $"{Type.Name} (\"{Key}\")";
        }
    }
}