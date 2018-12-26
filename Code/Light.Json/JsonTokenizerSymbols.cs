namespace Light.Json
{
    public static class JsonTokenizerSymbols
    {
        public const char StringDelimiter = '"';
        public const char EscapeCharacter = '\\';
        public const char FalseStartCharacter = 'f';
        public const string False = "false";
        public const char TrueStartCharacter = 't';
        public const string True = "true";
        public const char NullStartCharacter = 'n';
        public const string Null = "null";
    }
}