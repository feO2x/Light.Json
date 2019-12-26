namespace Light.Json.PrimitiveParsing
{
    public enum IntegerParseResult
    {
        NoNumber,
        Overflow,
        NonZeroDigitsAfterDecimalPoint,
        ParsingSuccessful
    }
}