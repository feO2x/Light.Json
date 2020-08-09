using Light.Json.Deserialization;
using Light.Json.Deserialization.Tokenization;
using Light.Json.Serialization;
using Light.Json.Serialization.LowLevelWriting;

namespace Light.Json.Contracts
{
    public abstract class SerializationContract<T> : BaseContract<T>, ISerializationContract<T>
    {
        protected SerializationContract(string? contractKey = null) : base(ContractKind.TwoWay, contractKey) { }

        public abstract void Serialize<TJsonWriter>(T @object, SerializationContext context, ref TJsonWriter writer)
            where TJsonWriter : struct, IJsonWriter;
        

        public void SerializeObject<TJsonWriter>(object @object, SerializationContext context, ref TJsonWriter writer)
            where TJsonWriter : struct, IJsonWriter
        {
            Serialize((T) @object, context, ref writer);
        }

        public abstract T Deserialize<TJsonTokenizer, TJsonToken>(DeserializationContext context, ref TJsonTokenizer tokenizer)
            where TJsonTokenizer : struct, IJsonTokenizer<TJsonToken> where TJsonToken : struct, IJsonToken;

        public object? DeserializeObject<TJsonTokenizer, TJsonToken>(DeserializationContext context, ref TJsonTokenizer tokenizer)
            where TJsonTokenizer : struct, IJsonTokenizer<TJsonToken> where TJsonToken : struct, IJsonToken
        {
            return Deserialize<TJsonTokenizer, TJsonToken>(context, ref tokenizer);
        }
    }
}
