namespace Light.Json.Tokenization
{
    public enum JsonTokenType
    {
        String = 1,
        IntegerNumber = 2,
        FloatingPointNumber = 3,
        BeginOfObject = 4,
        BeginOfArray = 5,
        EndOfObject = 6,
        EndOfArray = 7,
        NameValueSeparator = 8,
        EntrySeparator = 9,
        True = 10,
        False = 11,
        Null = 12,
        EndOfDocument = 13
    }
}