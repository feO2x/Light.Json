using Light.Json.Tokenization.Utf8;

namespace Light.Json.Parsing
{
    public struct JsonUtf8DeserializationContext : IDeserializationContext<JsonUtf8Tokenizer, JsonUtf8Token>
    {
        private readonly JsonDeserializer _deserializer;

        public JsonUtf8DeserializationContext(JsonUtf8Tokenizer tokenizer, JsonDeserializer deserializer)
        {
            Tokenizer = tokenizer;
            _deserializer = deserializer;
        }

        public JsonUtf8Tokenizer Tokenizer { get; }

        public T DeserializeChild<T>() =>
            _deserializer.Deserialize<T, JsonUtf8DeserializationContext, JsonUtf8Tokenizer, JsonUtf8Token>(ref this);
    }
}