namespace Light.Json.FrameworkExtensions
{
    public static class TextExtension
    {
        public static bool IsJsonDigit(this char character) =>
            character >= '0' && character <= '9';

        public static bool IsJsonDigitButNotZero(this char character) =>
            character >= '1' && character <= '9';

        public static bool IsJsonDigit(this byte character) =>
            character >= '0' && character <= '9';

        public static bool IsJsonDigitButNotZero(this byte character) =>
            character >= '1' && character <= '9';
    }
}