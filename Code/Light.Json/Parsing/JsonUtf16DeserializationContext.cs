using Light.Json.Tokenization.Utf16;

namespace Light.Json.Parsing
{
    public struct JsonUtf16DeserializationContext : IDeserializationContext<JsonUtf16Tokenizer, JsonUtf16Token>
    {
        private readonly JsonDeserializer _deserializer;

        public JsonUtf16DeserializationContext(JsonUtf16Tokenizer tokenizer, JsonDeserializer deserializer)
        {
            Tokenizer = tokenizer;
            _deserializer = deserializer;
        }

        public JsonUtf16Tokenizer Tokenizer { get; }

        public T DeserializeChild<T>() =>
            _deserializer.Deserialize<T, JsonUtf16DeserializationContext, JsonUtf16Tokenizer, JsonUtf16Token>(ref this);
    }
}