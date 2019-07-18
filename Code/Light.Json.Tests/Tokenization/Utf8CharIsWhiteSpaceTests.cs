using System.Text;
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
            var utf16String = whiteSpaceCharacter.ToString();
            var utf8Bytes = Encoding.UTF8.GetBytes(utf16String);
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
                theoryData.Add((char) Unicode.Space);
                for (var character = Unicode.HorizontalTab; character <= Unicode.CarriageReturn; ++character)
                {
                    theoryData.Add((char) character);
                }

                theoryData.Add((char) Unicode.NoBreakSpace);
                theoryData.Add((char) Unicode.NextLine);

                // Two byte white spaces
                theoryData.Add((char) Unicode.OghamSpaceMark);
                for (var character = Unicode.EnQuad; character <= Unicode.HairSpace; ++character)
                {
                    theoryData.Add((char) character);
                }
                theoryData.Add((char) Unicode.LineSeparator);
                theoryData.Add((char) Unicode.ParagraphSeparator);
                theoryData.Add((char) Unicode.NarrowNoBreakSpace);
                theoryData.Add((char) Unicode.MediumMathematicalSpace);
                theoryData.Add((char) Unicode.IdeographicSpace);
                theoryData.Add((char) Unicode.MongolianVowelSeparator);
                for (var character = Unicode.ZeroWidthSpace; character <= Unicode.ZeroWidthJoiner; ++character)
                {
                    theoryData.Add((char)character);
                }
                theoryData.Add((char) Unicode.WordJoiner);
                theoryData.Add((char) Unicode.ZeroWidthNonBreakingSpace);

                return theoryData;
            }
        }

        [Theory]
        [MemberData(nameof(NonWhiteSpaceCodes))]
        public static void IsNotWhiteSpace(char nonWhiteSpaceCharacter)
        {
            var utf16String = nonWhiteSpaceCharacter.ToString();
            var utf8Bytes = Encoding.UTF8.GetBytes(utf16String);
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