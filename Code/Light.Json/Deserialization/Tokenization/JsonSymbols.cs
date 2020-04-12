namespace Light.Json.Deserialization.Tokenization
{
    public static class JsonSymbols
    {
        public const char Space = ' ';
        public const char HorizontalTab = '\t';
        public const char LineFeed = '\n';
        public const char CarriageReturn = '\r';
        public const char QuotationMark = '"';
        public const char Backslash = '\\';
        public const char Slash = '/';
        public const char FalseFirstCharacter = 'f';
        public const string False = "false";
        public const char TrueFirstCharacter = 't';
        public const string True = "true";
        public const char NullFirstCharacter = 'n';
        public const string Null = "null";
        public const char MinusSign = '-';
        public const char DecimalSymbol = '.';
        public const char BeginOfObject = '{';
        public const char EndOfObject = '}';
        public const char BeginOfArray = '[';
        public const char EndOfArray = ']';
        public const char EntrySeparator = ',';
        public const char NameValueSeparator = ':';
        public const char SingleLineCommentCharacter = '/';
    }
}