using Light.GuardClauses;

namespace Light.Json.Deserialization
{
    public readonly struct DeserializationContext
    {
        public DeserializationContext(JsonSerializer serializer) =>
            Serializer = serializer.MustNotBeNull(nameof(serializer));

        public JsonSerializer Serializer { get; }
    }
}