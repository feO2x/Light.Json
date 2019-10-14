namespace Light.Json.Tokenization.Utf8
{
    public enum Utf8ParseResult
    {
        CharacterParsedSuccessfully,
        InsufficientBytes,
        ByteSequenceIsNotUtf8Compliant
    }
}