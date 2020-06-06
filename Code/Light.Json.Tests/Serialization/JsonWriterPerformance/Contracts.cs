using System;
using Light.GuardClauses;

namespace Light.Json.Tests.Serialization.JsonWriterPerformance
{
    public interface ISerializationContract
    {
        TypeKey TypeKey { get; }
    }

    public interface ISerializeOnlyContract : ISerializationContract
    {
        void SerializeObject<TJsonWriter>(object @object, SerializationContext context, ref TJsonWriter writer)
            where TJsonWriter : struct, IJsonWriter;
    }

    public interface ISerializeOnlyContract<in T> : ISerializeOnlyContract
    {
        void Serialize<TJsonWriter>(T @object, SerializationContext context, ref TJsonWriter writer)
            where TJsonWriter : struct, IJsonWriter;
    }

    public abstract class BaseContract<T> : ISerializationContract, IEquatable<ISerializationContract>
    {
        protected BaseContract(string? contractKey = null) =>
            TypeKey = new TypeKey(typeof(T), contractKey);

        public TypeKey TypeKey { get; }

        public bool Equals(ISerializationContract? other)
        {
            if (ReferenceEquals(this, other))
                return true;
            if (other is null)
                return false;
            return TypeKey.Equals(other.TypeKey);
        }

        public override bool Equals(object? obj) =>
            obj is ISerializationContract contract && Equals(contract);

        public override int GetHashCode() => TypeKey.GetHashCode();
    }

    public abstract class SerializeOnlyContract<T> : BaseContract<T>, ISerializeOnlyContract<T>
    {
        protected SerializeOnlyContract(string? contractKey = null) : base(contractKey) { }

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
        private readonly int _hashCode;

        public TypeKey(Type type, string? key = null)
        {
            Type = type.MustNotBeNull(nameof(Type));
            if (key.IsNullOrWhiteSpace())
            {
                Key = null;
                _hashCode = Type.GetHashCode();
            }
            else
            {
                Key = key;
                unchecked
                {
                    _hashCode = (type.GetHashCode() * 397) ^ key!.GetHashCode();
                }
            }
        }

        public Type Type { get; }

        public string? Key { get; }

        public bool Equals(TypeKey other) =>
            Type == other.Type && Key == other.Key;

        public override bool Equals(object? obj) =>
            obj is Contracts.TypeKey other && Equals(other);

        public override int GetHashCode() => _hashCode;

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

    public readonly struct SerializationContext { }
}