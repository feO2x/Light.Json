using Light.Json.Deserialization.Parsing;
using Light.Json.Deserialization.Tokenization;

namespace Light.Json.Contracts
{
    public interface IDeserializeOnlyContract : ISerializationContract
    {
        public object DeserializeObject<TJsonTokenizer, TJsonToken>(in DeserializationContext context, ref TJsonTokenizer tokenizer)
            where TJsonTokenizer : struct, IJsonTokenizer<TJsonToken>
            where TJsonToken : struct, IJsonToken;
    }

    public interface IDeserializeOnlyContract<out T> : IDeserializeOnlyContract
    {
        public T Deserialize<TJsonTokenizer, TJsonToken>(in DeserializationContext context, ref TJsonTokenizer tokenizer)
            where TJsonTokenizer : struct, IJsonTokenizer<TJsonToken>
            where TJsonToken : struct, IJsonToken;
    }
}