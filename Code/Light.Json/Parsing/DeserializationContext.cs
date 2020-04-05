using Light.GuardClauses;

namespace Light.Json.Parsing
{
    public readonly struct DeserializationContext
    {
        public DeserializationContext(JsonDeserializer deserializer) =>
            Deserializer = deserializer.MustNotBeNull(nameof(deserializer));

        public JsonDeserializer Deserializer { get; }
    }
}