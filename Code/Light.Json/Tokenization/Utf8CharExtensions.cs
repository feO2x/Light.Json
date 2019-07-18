using System.Runtime.CompilerServices;

namespace Light.Json.Tokenization
{
    public static class Utf8CharExtensions
    {
        // White space characters were retrieved here: https://en.wikipedia.org/wiki/Whitespace_character
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWhiteSpace(this in Utf8Char utf8Char)
        {
            if (utf8Char.Length != 1)
                return utf8Char.IsWhiteSpaceSlow();

            var @byte = utf8Char.Span[0];
            return @byte == Unicode.Space ||
                   @byte >= Unicode.HorizontalTab && @byte <= Unicode.CarriageReturn;
        }

        private static bool IsWhiteSpaceSlow(this in Utf8Char utf8Char)
        {
            if (!utf8Char.TryConvertTwoByteCharacter(out var utf16Value) &&
                !utf8Char.TryConvertThreeByteCharacter(out utf16Value))
            {
                return false;
            }

            return utf16Value == Unicode.NextLine ||
                   utf16Value == Unicode.NoBreakSpace ||
                   utf16Value == Unicode.OghamSpaceMark ||
                   utf16Value >= Unicode.EnQuad && utf16Value <= Unicode.HairSpace ||
                   utf16Value == Unicode.LineSeparator ||
                   utf16Value == Unicode.ParagraphSeparator ||
                   utf16Value == Unicode.NarrowNoBreakSpace ||
                   utf16Value == Unicode.MediumMathematicalSpace ||
                   utf16Value == Unicode.IdeographicSpace ||
                   utf16Value == Unicode.MongolianVowelSeparator ||
                   utf16Value >= Unicode.ZeroWidthSpace && utf16Value <= Unicode.ZeroWidthJoiner ||
                   utf16Value == Unicode.WordJoiner ||
                   utf16Value == Unicode.ZeroWidthNonBreakingSpace;
        }
    }
}