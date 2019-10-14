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
                return utf8Character.IsWhiteSpaceSlow();

            var @byte = utf8Character.Span[0];
            return @byte == JsonSymbols.Space ||
                   @byte >= JsonSymbols.HorizontalTab && @byte <= JsonSymbols.CarriageReturn;
        }

        private static bool IsWhiteSpaceSlow(this Utf8Character utf8Character)
        {
            var utf16Value = utf8Character.Utf16Code;

            return utf16Value == JsonSymbols.NextLine ||
                   utf16Value == JsonSymbols.NoBreakSpace ||
                   utf16Value == JsonSymbols.OghamSpaceMark ||
                   utf16Value >= JsonSymbols.EnQuad && utf16Value <= JsonSymbols.HairSpace ||
                   utf16Value == JsonSymbols.LineSeparator ||
                   utf16Value == JsonSymbols.ParagraphSeparator ||
                   utf16Value == JsonSymbols.NarrowNoBreakSpace ||
                   utf16Value == JsonSymbols.MediumMathematicalSpace ||
                   utf16Value == JsonSymbols.IdeographicSpace ||
                   utf16Value == JsonSymbols.MongolianVowelSeparator ||
                   utf16Value >= JsonSymbols.ZeroWidthSpace && utf16Value <= JsonSymbols.ZeroWidthJoiner ||
                   utf16Value == JsonSymbols.WordJoiner ||
                   utf16Value == JsonSymbols.ZeroWidthNonBreakingSpace;
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