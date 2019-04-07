using System.Text;
using FluentAssertions;
using Light.Json.Tokenization;
using Xunit;

namespace Light.Json.Tests.Tokenization
{
    public static class Utf8CharTests
    {
        [Theory]
        [MemberData(nameof(SingleByteCharacters))]
        public static void ParseSingleByteCharacters(string character) =>
            Parse(character, 1);

        public static TheoryData<string> SingleByteCharacters
        {
            get
            {
                var theoryData = new TheoryData<string>();
                for (var character = default(char); character < 128; ++character)
                    theoryData.Add(new string(character, 1));

                return theoryData;
            }
        }

        [Theory]
        [MemberData(nameof(TwoByteCharacters))]
        public static void ParseTwoByteCharacters(string character) =>
            Parse(character, 2);

        public static TheoryData<string> TwoByteCharacters
        {
            get
            {
                var theoryData = new TheoryData<string>();
                char character;
                // First 50 two-byte characters
                for (character = (char) 128; character < 178; ++character)
                    theoryData.Add(new string(character, 1));


                // Last 50 two-byte characters
                for (character = (char) 1998; character < 2048; ++character)
                    theoryData.Add(new string(character, 1));
                return theoryData;
            }
        }

        [Theory]
        [MemberData(nameof(ThreeByteCharacters))]
        public static void ParseThreeByteCharacters(string character) =>
            Parse(character, 3);

        public static TheoryData<string> ThreeByteCharacters
        {
            get
            {
                var theoryData = new TheoryData<string>();
                char character;

                // First 50 three-byte characters
                for (character = (char) 2048; character < 2098; ++character)
                    theoryData.Add(new string(character, 1));

                // Last 50 three-byte characters
                character = (char) (char.MaxValue - 50);
                for (var i = 0; i < 50; ++i)
                    theoryData.Add(new string(character++, 1));

                return theoryData;
            }
        }

        [Theory]
        [MemberData(nameof(FourByteCharacters))]
        public static void ParseFourByteCharacters(string character) =>
            Parse(character, 4);

        public static TheoryData<string> FourByteCharacters
        {
            get
            {
                var theoryData = new TheoryData<string>();
                int utf32Character;

                // First 50 four-byte characters
                var lowerBoundary = char.MaxValue + 1;
                var upperBoundary = lowerBoundary + 50;
                for (utf32Character = lowerBoundary; utf32Character < upperBoundary; ++utf32Character)
                    theoryData.Add(char.ConvertFromUtf32(utf32Character));

                // Last 50 four-byte characters
                upperBoundary = 1_114_111 + 1; // 1,114,111 is the highest Unicode code point
                lowerBoundary = upperBoundary - 50;
                for (utf32Character = lowerBoundary; utf32Character < upperBoundary; ++utf32Character)
                    theoryData.Add(char.ConvertFromUtf32(utf32Character));

                return theoryData;
            }
        }

        private static void Parse(string character, int expectedLength)
        {
            var bytes = Encoding.UTF8.GetBytes(character);

            var result = Utf8Char.TryParseNext(bytes, out var utf8Char);

            bytes.Length.Should().Be(expectedLength);
            utf8Char.Span.MustEqual(bytes);
            result.Should().BeTrue();
        }
    }
}