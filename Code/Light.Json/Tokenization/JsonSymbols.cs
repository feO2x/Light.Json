namespace Light.Json.Tokenization
{
    public static class JsonSymbols
    {
        // White Space
        public const char Space = '\x0020';
        public const char HorizontalTab = '\x0009';
        public const char LineFeed = '\x000a';
        public const char CarriageReturn = '\x000d';
        public const char NoBreakSpace = '\x00a0';
        public const char NextLine = '\x0085';
        // ReSharper disable once IdentifierTypo
        public const char OghamSpaceMark = '\x1680';
        public const char EnQuad = '\x2000';
        public const char HairSpace = '\x200A';
        public const char LineSeparator = '\x2028';
        public const char ParagraphSeparator = '\x2029';
        public const char NarrowNoBreakSpace = '\x202F';
        public const char MediumMathematicalSpace = '\x205F';
        public const char IdeographicSpace = '\x3000';
        public const char MongolianVowelSeparator = '\x180E';
        public const char ZeroWidthSpace = '\x200B';
        public const char ZeroWidthJoiner = '\x200D';
        public const char WordJoiner = '\x2060';
        public const char ZeroWidthNonBreakingSpace = '\xFEFF';

        // JSON symbols
        public const char StringDelimiter = '"';
        public const char EscapeCharacter = '\\';
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