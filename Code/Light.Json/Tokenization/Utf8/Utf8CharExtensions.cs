using System.Runtime.CompilerServices;
using System.Text;

namespace Light.Json.Tokenization.Utf8
{
    public static class Utf8CharExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToUtf8(this char character) => Encoding.UTF8.GetBytes(character.ToString());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ToUtf8(this string @string) => Encoding.UTF8.GetBytes(@string);

        // White space characters were retrieved here: https://en.wikipedia.org/wiki/Whitespace_character
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWhiteSpace(this in Utf8Char utf8Char)
        {
            if (utf8Char.Length != 1)
                return utf8Char.IsWhiteSpaceSlow();

            var @byte = utf8Char.Span[0];
            return @byte == UnicodeConstants.Space ||
                   @byte >= UnicodeConstants.HorizontalTab && @byte <= UnicodeConstants.CarriageReturn;
        }

        private static bool IsWhiteSpaceSlow(this in Utf8Char utf8Char)
        {
            if (!utf8Char.TryConvertTwoByteCharacter(out var utf16Value) &&
                !utf8Char.TryConvertThreeByteCharacter(out utf16Value))
            {
                return false;
            }

            return utf16Value == UnicodeConstants.NextLine ||
                   utf16Value == UnicodeConstants.NoBreakSpace ||
                   utf16Value == UnicodeConstants.OghamSpaceMark ||
                   utf16Value >= UnicodeConstants.EnQuad && utf16Value <= UnicodeConstants.HairSpace ||
                   utf16Value == UnicodeConstants.LineSeparator ||
                   utf16Value == UnicodeConstants.ParagraphSeparator ||
                   utf16Value == UnicodeConstants.NarrowNoBreakSpace ||
                   utf16Value == UnicodeConstants.MediumMathematicalSpace ||
                   utf16Value == UnicodeConstants.IdeographicSpace ||
                   utf16Value == UnicodeConstants.MongolianVowelSeparator ||
                   utf16Value >= UnicodeConstants.ZeroWidthSpace && utf16Value <= UnicodeConstants.ZeroWidthJoiner ||
                   utf16Value == UnicodeConstants.WordJoiner ||
                   utf16Value == UnicodeConstants.ZeroWidthNonBreakingSpace;
        }
    }
}