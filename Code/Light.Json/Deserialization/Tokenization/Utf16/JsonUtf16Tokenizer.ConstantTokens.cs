using System.Runtime.Serialization;

namespace Light.Json.Deserialization.Tokenization.Utf16
{
    public partial struct JsonUtf16Tokenizer
    {
        public void ReadBeginOfObject() =>
            ReadSingleCharacter(JsonSymbols.BeginOfObject);

        public void ReadEndOfObject() =>
            ReadSingleCharacter(JsonSymbols.EndOfObject);

        public void ReadNameValueSeparator() => 
            ReadSingleCharacter(JsonSymbols.NameValueSeparator);

        private void ReadSingleCharacter(char expectedCharacter)
        {
            var json = _json.Span;
            if (!TryReadNextCharacter(json, out var currentCharacter))
                throw new SerializationException($"Expected \"{expectedCharacter}\" but found end of JSON document.");
            if (expectedCharacter != currentCharacter)
                throw new SerializationException($"Expected \"{expectedCharacter}\" but found \"{currentCharacter}\".");

            ++CurrentIndex;
            ++CurrentPosition;
        }
    }
}