using System;

namespace Light.Json.Contracts
{
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
}