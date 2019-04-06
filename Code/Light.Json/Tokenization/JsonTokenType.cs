namespace Light.Json.Tokenization
{
    public enum JsonTokenType
    {
        String = 1,
        IntegerNumber,
        FloatingPointNumber,
        BeginOfObject,
        BeginOfArray,
        EndOfObject,
        EndOfArray,
        NameValueSeparator,
        EntrySeparator,
        True,
        False,
        Null,
        EndOfDocument
    }
}