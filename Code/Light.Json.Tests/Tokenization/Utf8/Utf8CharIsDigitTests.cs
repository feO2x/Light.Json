using FluentAssertions;
using Light.Json.Tokenization.Utf8;
using Xunit;

namespace Light.Json.Tests.Tokenization.Utf8
{
    public static class Utf8CharIsDigitTests
    {
        [Theory]
        [MemberData(nameof(IsDigitData))]
        public static void IsDigit(char character, bool expected)
        {
            Utf8Char.TryParseNext(character.ToUtf8(), out var utf8Char);

            utf8Char.IsDigit().Should().Be(expected);
        }

        public static TheoryData<char, bool> IsDigitData
        {
            get
            {
                var theoryData = new TheoryData<char, bool>();
                for (var i = '0'; i <= '9'; ++i)
                {
                    theoryData.Add(i, true);
                }

                theoryData.Add('a', false);
                theoryData.Add('b', false);
                theoryData.Add('X', false);
                theoryData.Add('Y', false);
                theoryData.Add('!', false);
                theoryData.Add('$', false);
                theoryData.Add('Ü', false);
                theoryData.Add('ä', false);
                theoryData.Add('Ö', false);
                theoryData.Add('€', false);
                theoryData.Add('µ', false);

                return theoryData;
            }
        }
    }
}