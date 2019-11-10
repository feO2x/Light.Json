using FluentAssertions;
using Light.Json.Tokenization;
using Light.Json.Tokenization.Utf8;
using Xunit;

namespace Light.Json.Tests.Tokenization.Utf8
{
    public static class Utf8CharacterIsWhiteSpaceTests
    {
        [Theory]
        [InlineData(JsonSymbols.Space)]
        [InlineData(JsonSymbols.HorizontalTab)]
        [InlineData(JsonSymbols.CarriageReturn)]
        [InlineData(JsonSymbols.LineFeed)]
        public static void IsWhiteSpace(char whiteSpaceCharacter)
        {
            var utf8Bytes = whiteSpaceCharacter.ToUtf8();
            Utf8Character.TryParseNext(utf8Bytes, out var utf8Char);

            var result = utf8Char.IsWhiteSpace();

            result.Should().BeTrue();
        }

        [Theory]
        [MemberData(nameof(NonWhiteSpaceCodes))]
        public static void IsNotWhiteSpace(char nonWhiteSpaceCharacter)
        {
            var utf8Bytes = nonWhiteSpaceCharacter.ToUtf8();
            Utf8Character.TryParseNext(utf8Bytes, out var utf8Char);

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