namespace Light.Json.Tokenization.Utf8
{
    public enum Utf8ParseResult
    {
        StartIndexOutOfRange,
        CharacterParsedSuccessfully,
        InsufficientBytes,
        ByteSequenceIsNotUtf8Compliant
    }
}