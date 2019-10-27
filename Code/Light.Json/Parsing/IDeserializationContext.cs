using Light.Json.Tokenization;

namespace Light.Json.Parsing
{
    public interface IDeserializationContext<out TJsonTokenizer, TJsonToken>
        where TJsonTokenizer : struct, IJsonTokenizer<TJsonToken>
        where TJsonToken : struct, IJsonToken
    {
        TJsonTokenizer Tokenizer { get; }

        T DeserializeChild<T>();
    }
}