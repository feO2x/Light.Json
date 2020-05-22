using System;
using System.Globalization;
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

        public static string LowerFirstCharacter(this string @string)
        {
            if (@string.MustNotBeNull(nameof(@string)).Length == 0)
                return @string;

            if (char.IsLower(@string[0]))
                return @string;

            var loweredCharacter = char.ToLower(@string[0], CultureInfo.CurrentCulture);
            if (@string.Length == 0)
                return new string(loweredCharacter, 1);

            Span<char> characters = stackalloc char[@string.Length];
            characters[0] = loweredCharacter;
            @string.AsSpan(1).CopyTo(characters.Slice(1));
            return characters.ToString();
        }
    }
}