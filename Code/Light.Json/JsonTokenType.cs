namespace Light.Json
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
        ValueDelimiter,
        PairDelimiter,
        True,
        False,
        Null,
        EndOfDocument
    }
}