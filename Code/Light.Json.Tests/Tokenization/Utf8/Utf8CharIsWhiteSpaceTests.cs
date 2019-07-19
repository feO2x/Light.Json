using FluentAssertions;
using Light.Json.Tokenization;
using Light.Json.Tokenization.Utf8;
using Xunit;

namespace Light.Json.Tests.Tokenization.Utf8
{
    public static class Utf8CharIsWhiteSpaceTests
    {
        [Theory]
        [MemberData(nameof(UnicodeWhiteSpace))]
        public static void IsWhiteSpace(char whiteSpaceCharacter)
        {
            var utf8Bytes = whiteSpaceCharacter.ToUtf8();
            Utf8Char.TryParseNext(utf8Bytes, out var utf8Char);

            var result = utf8Char.IsWhiteSpace();

            result.Should().BeTrue();
        }

        public static TheoryData<char> UnicodeWhiteSpace
        {
            get
            {
                // ReSharper disable once UseObjectOrCollectionInitializer
                var theoryData = new TheoryData<char>();

                // Single byte white space
                theoryData.Add(UnicodeConstants.Space);
                for (var character = UnicodeConstants.HorizontalTab; character <= UnicodeConstants.CarriageReturn; ++character)
                {
                    theoryData.Add(character);
                }

                theoryData.Add(UnicodeConstants.NoBreakSpace);
                theoryData.Add(UnicodeConstants.NextLine);

                // Two byte white spaces
                theoryData.Add(UnicodeConstants.OghamSpaceMark);
                for (var character = UnicodeConstants.EnQuad; character <= UnicodeConstants.HairSpace; ++character)
                {
                    theoryData.Add(character);
                }

                theoryData.Add(UnicodeConstants.LineSeparator);
                theoryData.Add(UnicodeConstants.ParagraphSeparator);
                theoryData.Add(UnicodeConstants.NarrowNoBreakSpace);
                theoryData.Add(UnicodeConstants.MediumMathematicalSpace);
                theoryData.Add(UnicodeConstants.IdeographicSpace);
                theoryData.Add(UnicodeConstants.MongolianVowelSeparator);
                for (var character = UnicodeConstants.ZeroWidthSpace; character <= UnicodeConstants.ZeroWidthJoiner; ++character)
                {
                    theoryData.Add(character);
                }

                theoryData.Add(UnicodeConstants.WordJoiner);
                theoryData.Add(UnicodeConstants.ZeroWidthNonBreakingSpace);

                return theoryData;
            }
        }

        [Theory]
        [MemberData(nameof(NonWhiteSpaceCodes))]
        public static void IsNotWhiteSpace(char nonWhiteSpaceCharacter)
        {
            var utf8Bytes = nonWhiteSpaceCharacter.ToUtf8();
            Utf8Char.TryParseNext(utf8Bytes, out var utf8Char);

            var result = utf8Char.IsWhiteSpace();

            result.Should().BeFalse();
        }

        public static TheoryData<char> NonWhiteSpaceCodes
        {
            get
            {
                var theoryData = new TheoryData<char>();
                for (var character = 'a'; character <= 'z'; ++character)
                {
                    theoryData.Add(character);
                }

                for (var character = 'A'; character <= 'Z'; ++character)
                {
                    theoryData.Add(character);
                }

                for (var character = '0'; character < '9'; ++character)
                {
                    theoryData.Add(character);
                }

                theoryData.Add('ß');
                theoryData.Add('ö');
                theoryData.Add('ä');
                theoryData.Add('ü');
                theoryData.Add('Ö');
                theoryData.Add('Ä');
                theoryData.Add('Ü');
                theoryData.Add('?');
                theoryData.Add('$');
                theoryData.Add('/');
                theoryData.Add('!');
                theoryData.Add('§');
                theoryData.Add('@');
                theoryData.Add('€');
                theoryData.Add('µ');

                return theoryData;
            }
        }
    }
}