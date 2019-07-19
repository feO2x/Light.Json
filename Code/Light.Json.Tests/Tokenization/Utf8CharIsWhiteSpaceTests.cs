using FluentAssertions;
using Light.Json.Tokenization;
using Xunit;

namespace Light.Json.Tests.Tokenization
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
                theoryData.Add(ImportantUnicode.Space);
                for (var character = ImportantUnicode.HorizontalTab; character <= ImportantUnicode.CarriageReturn; ++character)
                {
                    theoryData.Add(character);
                }

                theoryData.Add(ImportantUnicode.NoBreakSpace);
                theoryData.Add(ImportantUnicode.NextLine);

                // Two byte white spaces
                theoryData.Add(ImportantUnicode.OghamSpaceMark);
                for (var character = ImportantUnicode.EnQuad; character <= ImportantUnicode.HairSpace; ++character)
                {
                    theoryData.Add(character);
                }

                theoryData.Add(ImportantUnicode.LineSeparator);
                theoryData.Add(ImportantUnicode.ParagraphSeparator);
                theoryData.Add(ImportantUnicode.NarrowNoBreakSpace);
                theoryData.Add(ImportantUnicode.MediumMathematicalSpace);
                theoryData.Add(ImportantUnicode.IdeographicSpace);
                theoryData.Add(ImportantUnicode.MongolianVowelSeparator);
                for (var character = ImportantUnicode.ZeroWidthSpace; character <= ImportantUnicode.ZeroWidthJoiner; ++character)
                {
                    theoryData.Add(character);
                }

                theoryData.Add(ImportantUnicode.WordJoiner);
                theoryData.Add(ImportantUnicode.ZeroWidthNonBreakingSpace);

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