using Light.Json.Deserialization;
using Light.Json.Deserialization.Tokenization;

namespace Light.Json.Contracts
{
    public abstract class DeserializeOnlyContract<T> : IDeserializeOnlyContract<T>
    {
        protected DeserializeOnlyContract(string? contractKey = null) =>
            TypeKey = new TypeKey(typeof(T), contractKey);

        public TypeKey TypeKey { get; }

        public abstract T Deserialize<TJsonTokenizer, TJsonToken>(in DeserializationContext context,
                                                                  ref TJsonTokenizer tokenizer)
            where TJsonTokenizer : struct, IJsonTokenizer<TJsonToken>
            where TJsonToken : struct, IJsonToken;


        public object? DeserializeObject<TJsonTokenizer, TJsonToken>(in DeserializationContext context, ref TJsonTokenizer tokenizer)
            where TJsonTokenizer : struct, IJsonTokenizer<TJsonToken>
            where TJsonToken : struct, IJsonToken
        {
            return Deserialize<TJsonTokenizer, TJsonToken>(context, ref tokenizer);
        }
    }
}