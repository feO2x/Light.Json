using Light.GuardClauses;

namespace Light.Json.FrameworkExtensions
{
    public static class TextExtension
    {
        public static bool IsJsonDigit(this char character) =>
            character >= '0' && character <= '9';

        public static bool IsJsonDigit(this byte character) =>
            character >= '0' && character <= '9';

        public static bool IsWhiteSpace(this byte utf8Character) => ((char) utf8Character).IsWhiteSpace();
    }
}