namespace Light.Json.Tokenization
{
    public static class ImportantUnicode
    {
        // White Space
        public const char Space = '\x0020';
        public const char HorizontalTab = '\x0009';
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
        public const char SingleLineCommentCharacter = '/';

    }
}