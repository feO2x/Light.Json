using Light.Json.Serialization;
using Light.Json.Serialization.LowLevelWriting;

namespace Light.Json.Contracts
{
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
}