﻿namespace Light.Json.Tokenization.Utf8
{
    public partial struct JsonUtf8Tokenizer
    {
        public void ReadBeginOfObject() =>
            ReadSingleCharacter((byte) JsonSymbols.BeginOfObject);

        public void ReadEndOfObject() =>
            ReadSingleCharacter((byte) JsonSymbols.EndOfObject);

        public void ReadNameValueSeparator() => 
            ReadSingleCharacter((byte) JsonSymbols.NameValueSeparator);

        private void ReadSingleCharacter(byte expectedCharacter)
        {
            var json = _json.Span;
            if (!TryReadNextByte(json, out var currentCharacter))
                throw new DeserializationException($"Expected \"{(char) expectedCharacter}\" but found end of JSON document.");
            if (expectedCharacter != currentCharacter)
                throw new DeserializationException($"Expected \"{(char) expectedCharacter}\" but found \"{(char) currentCharacter}\".");

            ++CurrentIndex;
            ++CurrentPosition;
        }
    }
}