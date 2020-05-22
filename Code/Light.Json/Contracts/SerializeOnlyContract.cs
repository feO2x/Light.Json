using Light.Json.Serialization;
using Light.Json.Serialization.LowLevelWriting;

namespace Light.Json.Contracts
{
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
}