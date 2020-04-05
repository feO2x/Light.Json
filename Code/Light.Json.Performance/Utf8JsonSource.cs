using Light.GuardClauses;
using Light.Json.FrameworkExtensions;

namespace Light.Json.Performance
{
    public readonly struct Utf8JsonSource
    {
        public readonly byte[] Utf8Json;
        private readonly string _utf16Json;

        public Utf8JsonSource(string utf16Json)
        {
            _utf16Json = utf16Json.MustNotBeNullOrEmpty(nameof(utf16Json));
            Utf8Json = utf16Json.ToUtf8();
        }

        public override string ToString() => _utf16Json;

        public static implicit operator Utf8JsonSource(string utf16Source) =>
            new Utf8JsonSource(utf16Source);
    }
}