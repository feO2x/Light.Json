namespace Light.Json.Tokenization
{
    public enum Utf8ParseResult
    {
        CharacterParsedSuccessfully,
        InvalidStartIndex,
        InsufficientBytes,
        InvalidFirstByte
    }
}