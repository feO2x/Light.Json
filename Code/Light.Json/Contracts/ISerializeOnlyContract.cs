using Light.Json.Serialization;
using Light.Json.Serialization.LowLevelWriting;

namespace Light.Json.Contracts
{
    public interface ISerializeOnlyContract : ISerializationContract
    {
        public void SerializeObject<TJsonWriter>(object @object, SerializationContext context, ref TJsonWriter writer)
            where TJsonWriter : struct, IJsonWriter;
    }

    public interface ISerializeOnlyContract<in T> : ISerializationContract
    {
        public void Serialize<TJsonWriter>(T @object, SerializationContext context, ref TJsonWriter writer)
            where TJsonWriter : struct, IJsonWriter;
    }
}