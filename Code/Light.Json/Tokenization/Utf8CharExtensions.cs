using System.Runtime.CompilerServices;
using System.Text;

namespace Light.Json.Tokenization
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
            return @byte == ImportantUnicode.Space ||
                   @byte >= ImportantUnicode.HorizontalTab && @byte <= ImportantUnicode.CarriageReturn;
        }

        private static bool IsWhiteSpaceSlow(this in Utf8Char utf8Char)
        {
            if (!utf8Char.TryConvertTwoByteCharacter(out var utf16Value) &&
                !utf8Char.TryConvertThreeByteCharacter(out utf16Value))
            {
                return false;
            }

            return utf16Value == ImportantUnicode.NextLine ||
                   utf16Value == ImportantUnicode.NoBreakSpace ||
                   utf16Value == ImportantUnicode.OghamSpaceMark ||
                   utf16Value >= ImportantUnicode.EnQuad && utf16Value <= ImportantUnicode.HairSpace ||
                   utf16Value == ImportantUnicode.LineSeparator ||
                   utf16Value == ImportantUnicode.ParagraphSeparator ||
                   utf16Value == ImportantUnicode.NarrowNoBreakSpace ||
                   utf16Value == ImportantUnicode.MediumMathematicalSpace ||
                   utf16Value == ImportantUnicode.IdeographicSpace ||
                   utf16Value == ImportantUnicode.MongolianVowelSeparator ||
                   utf16Value >= ImportantUnicode.ZeroWidthSpace && utf16Value <= ImportantUnicode.ZeroWidthJoiner ||
                   utf16Value == ImportantUnicode.WordJoiner ||
                   utf16Value == ImportantUnicode.ZeroWidthNonBreakingSpace;
        }
    }
}