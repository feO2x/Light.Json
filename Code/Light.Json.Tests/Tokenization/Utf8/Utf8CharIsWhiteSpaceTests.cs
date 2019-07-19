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
                theoryData.Add(JsonSymbols.Space);
                for (var character = JsonSymbols.HorizontalTab; character <= JsonSymbols.CarriageReturn; ++character)
                {
                    theoryData.Add(character);
                }

                theoryData.Add(JsonSymbols.NoBreakSpace);
                theoryData.Add(JsonSymbols.NextLine);

                // Two byte white spaces
                theoryData.Add(JsonSymbols.OghamSpaceMark);
                for (var character = JsonSymbols.EnQuad; character <= JsonSymbols.HairSpace; ++character)
                {
                    theoryData.Add(character);
                }

                theoryData.Add(JsonSymbols.LineSeparator);
                theoryData.Add(JsonSymbols.ParagraphSeparator);
                theoryData.Add(JsonSymbols.NarrowNoBreakSpace);
                theoryData.Add(JsonSymbols.MediumMathematicalSpace);
                theoryData.Add(JsonSymbols.IdeographicSpace);
                theoryData.Add(JsonSymbols.MongolianVowelSeparator);
                for (var character = JsonSymbols.ZeroWidthSpace; character <= JsonSymbols.ZeroWidthJoiner; ++character)
                {
                    theoryData.Add(character);
                }

                theoryData.Add(JsonSymbols.WordJoiner);
                theoryData.Add(JsonSymbols.ZeroWidthNonBreakingSpace);

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