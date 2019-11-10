using System.Text;

namespace Light.Json.Tokenization.Utf8
{
    public static class Utf8CharacterExtensions
    {
        public static byte[] ToUtf8(this char character) => Encoding.UTF8.GetBytes(character.ToString());

        public static byte[] ToUtf8(this string @string) => Encoding.UTF8.GetBytes(@string);

        // White space characters were retrieved here: https://en.wikipedia.org/wiki/Whitespace_character
        public static bool IsWhiteSpace(this Utf8Character utf8Character)
        {
            if (utf8Character.ByteLength != 1)
                return false;

            var @byte = utf8Character.Span[0];
            return @byte == JsonSymbols.Space ||
                   @byte == JsonSymbols.HorizontalTab ||
                   @byte == JsonSymbols.CarriageReturn ||
                   @byte == JsonSymbols.LineFeed;

        }

        public static bool IsDigit(this in Utf8Character utf8Character)
        {
            if (utf8Character.ByteLength != 1)
                return false;

            var @byte = utf8Character.Span[0];
            return @byte >= '0' && @byte <= '9';
        }
    }
}