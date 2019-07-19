using System;
using System.Text;
using FluentAssertions;
using Light.Json.Tokenization.Utf8;
using Xunit;

namespace Light.Json.Tests.Tokenization.Utf8
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
            result.Should().Be(Utf8ParseResult.CharacterParsedSuccessfully);
        }

        [Fact]
        public static void FollowUpByte()
        {
            // This byte cannot be the first byte of a UTF-8 character because 
            // it starts with 10
            var bytes = new byte[] { 0b1001_1101 };

            var result = Utf8Char.TryParseNext(bytes, out var character);

            result.Should().Be(Utf8ParseResult.InvalidStartIndex);
            character.MustBeDefault();
        }

        [Fact]
        public static void InvalidFirstByte()
        {
            // This byte has too many true bits at the beginning
            var bytes = new byte[] { 0b1111_1110 };

            var result = Utf8Char.TryParseNext(bytes, out var character);

            result.Should().Be(Utf8ParseResult.InvalidFirstByte);
            character.MustBeDefault();
        }

        [Fact]
        public static void InsufficientBytes()
        {
            var fourByteCharacter = char.ConvertFromUtf32(1_114_111);
            var bytes = Encoding.UTF8.GetBytes(fourByteCharacter);

            var result = Utf8Char.TryParseNext(bytes.AsSpan().Slice(0, 3), out var character);

            result.Should().Be(Utf8ParseResult.InsufficientBytes);
            character.MustBeDefault();
        }

        [Fact]
        public static void StartIndexTooLow()
        {
            var bytes = Encoding.UTF8.GetBytes("foo");

            var result = Utf8Char.TryParseNext(bytes, out _, -1);

            result.Should().Be(Utf8ParseResult.InvalidStartIndex);
        }

        [Fact]
        public static void StartIndexTooHigh()
        {
            var bytes = Encoding.UTF8.GetBytes("bar");

            var result = Utf8Char.TryParseNext(bytes, out _, bytes.Length);

            result.Should().Be(Utf8ParseResult.InvalidStartIndex);
        }

        [Theory]
        [InlineData('a')]
        [InlineData('A')]
        [InlineData('1')]
        [InlineData('\t')]
        [InlineData(char.MaxValue)]
        public static void EqualsUtf16Character(char utf16Character)
        {
            var bytes = Encoding.UTF8.GetBytes(new string(utf16Character, 1));
            Utf8Char.TryParseNext(bytes, out var utf8Character);

            utf8Character.Equals(utf16Character).Should().BeTrue();
        }

        [Theory]
        [InlineData('a', 'b', false)]
        [InlineData('C', 'C', true)]
        [InlineData('d', 'D', false)]
        [InlineData('x', 'x', true)]
        public static void EqualsOperator(char x, char y, bool expectedResult)
        {
            var xBytes = Encoding.UTF8.GetBytes(new string(x, 1));
            var yBytes = Encoding.UTF8.GetBytes(new string(y, 1));
            Utf8Char.TryParseNext(xBytes, out var xUtf8Char);
            Utf8Char.TryParseNext(yBytes, out var yUtf8Char);

            (xUtf8Char == yUtf8Char).Should().Be(expectedResult);
            (xUtf8Char != yUtf8Char).Should().Be(!expectedResult);
        }
    }
}