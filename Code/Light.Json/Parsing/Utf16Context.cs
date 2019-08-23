using Light.Json.Tokenization.Utf16;

namespace Light.Json.Parsing
{
    public readonly ref struct Utf16Context
    {
        public readonly JsonUtf16Tokenizer Tokenizer;

        public Utf16Context(JsonUtf16Tokenizer tokenizer)
        {
            Tokenizer = tokenizer;
        }
    }
}