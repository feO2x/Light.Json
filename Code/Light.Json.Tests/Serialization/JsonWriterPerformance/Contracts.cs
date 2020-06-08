using System;
using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.Json.Tests.Serialization.JsonWriterPerformance
{
    public abstract class BaseContract : IEquatable<BaseContract>
    {
        protected BaseContract(TypeKey typeKey)
        {
            TypeKey = typeKey;
        }

        public TypeKey TypeKey { get; }


        public bool Equals(BaseContract? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return TypeKey.Equals(other.TypeKey);
        }

        public override bool Equals(object? obj) =>
            obj is BaseContract contract && Equals(contract);

        public override int GetHashCode() => TypeKey.GetHashCode();
    }

    public interface ISerializeOnlyContract
    {
        void SerializeObject<TJsonWriter>(object @object, SerializationContext context, ref TJsonWriter writer)
            where TJsonWriter : struct, IJsonWriter;
    }

    public interface ISerializeOnlyContract<in T> : ISerializeOnlyContract
    {
        void Serialize<TJsonWriter>(T @object, SerializationContext context, ref TJsonWriter writer)
            where TJsonWriter : struct, IJsonWriter;
    }

    public abstract class SerializeOnlyContract<T> : BaseContract, ISerializeOnlyContract<T>
    {
        protected SerializeOnlyContract(string? contractKey = null) : base(new TypeKey(typeof(T), contractKey)) { }

        public void SerializeObject<TJsonWriter>(object @object, SerializationContext context, ref TJsonWriter writer)
            where TJsonWriter : struct, IJsonWriter
        {
            Serialize((T) @object, context, ref writer);
        }

        public abstract void Serialize<TJsonWriter>(T @object, SerializationContext context, ref TJsonWriter writer)
            where TJsonWriter : struct, IJsonWriter;

        public override string ToString() => "Serialize-Only-Contract " + TypeKey;
    }

    public readonly struct TypeKey : IEquatable<TypeKey>
    {
        public TypeKey(Type type, string? key = null)
        {
            Type = type.MustNotBeNull(nameof(Type));
            if (key.IsNullOrWhiteSpace())
            {
                Key = null;
                HashCode = unchecked((uint) Type.GetHashCode());
            }
            else
            {
                Key = key;
                unchecked
                {
                    HashCode = (uint) ((type.GetHashCode() * 397) ^ key!.GetHashCode());
                }
            }
        }


        public uint HashCode { get; }
        public string? Key { get; }
        public Type Type { get; }

        public bool Equals(TypeKey other) =>
            Type == other.Type && Key == other.Key;

        public override bool Equals(object? obj) =>
            obj is Contracts.TypeKey other && Equals(other);

        public override int GetHashCode() => (int) HashCode;

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

        public sealed class EqualityComparer : IEqualityComparer<TypeKey>
        {
            public static readonly EqualityComparer Instance = new EqualityComparer();

            public bool Equals(TypeKey x, TypeKey y) => x.Type == y.Type && x.Key == y.Key;

            public int GetHashCode(TypeKey typeKey) => unchecked((int) typeKey.HashCode);
        }
    }

    public readonly struct SerializationContext { }
}