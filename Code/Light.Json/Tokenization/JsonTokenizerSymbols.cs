﻿namespace Light.Json.Tokenization
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
        public const char MinusSign = '-';
        public const char DecimalSymbol = '.';
        public const char BeginOfObject = '{';
        public const char EndOfObject = '}';
        public const char BeginOfArray = '[';
        public const char EndOfArray = ']';
        public const char EntrySeparator = ',';
        public const char NameValueSeparator = ':';
        public const string SingleLineComment = "//";
        public const char SingleLineCommentCharacter = '/';
    }
}