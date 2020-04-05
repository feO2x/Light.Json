using Light.Json.PrimitiveParsing;

namespace Light.Json.Tokenization.Utf16
{
    public partial struct JsonUtf16Tokenizer
    {
        public int ReadInt32()
        {
            var json = _json.Span;
            if (!TrySkipWhiteSpace(json))
                throw new DeserializationException("Expected JSON integer number but found end of document.");
            json = json.Slice(CurrentIndex);
            var result = json.TryParseInt32(out var value, out var bytesConsumed);
            switch (result)
            {
                case IntegerParseResult.ParsingSuccessful:
                    CurrentIndex += bytesConsumed;
                    CurrentPosition += bytesConsumed;
                    return value;
                case IntegerParseResult.NoNumber:
                    throw new DeserializationException($"Expected JSON integer number but token \"{GetErroneousToken()}\" at line {CurrentLine} position {CurrentPosition} does not represent a number.");
                default:
                    throw new DeserializationException($"The token \"{GetErroneousToken()}\" at line {CurrentLine} position {CurrentPosition} produces an overflow when parsing it to System.Int32.");
            }
        }
    }
}