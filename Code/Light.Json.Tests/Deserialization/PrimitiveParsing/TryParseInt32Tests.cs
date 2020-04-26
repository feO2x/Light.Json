using System;
using FluentAssertions;
using Light.Json.Deserialization.PrimitiveParsing;
using Light.Json.FrameworkExtensions;
using Xunit;

namespace Light.Json.Tests.Deserialization.PrimitiveParsing
{
    public static class TryParseInt32Tests
    {
        [Theory]
        [MemberData(nameof(ValidInt32Data))]
        public static void ValidInt32Utf8(int number) =>
            TestValidNumberUtf8(number.ToString(), number);

        [Theory]
        [MemberData(nameof(ValidInt32Data))]
        public static void ValidInt32Utf16(int number) =>
            TestValidNumberUtf16(number.ToString(), number);

        public static readonly TheoryData<int> ValidInt32Data =
            new TheoryData<int>
            {
                0,
                1,
                -1,
                int.MinValue,
                int.MaxValue,
                int.MinValue / 10,
                int.MinValue + 1,
                int.MaxValue / 10,
                int.MaxValue - 1
            };

        [Theory]
        [MemberData(nameof(InvalidNumberData))]
        public static void InvalidInt32Utf8(string invalidNumber)
        {
            var span = new ReadOnlySpan<byte>(invalidNumber.ToUtf8());

            var result = span.TryParseInt32(out _, out _);

            result.Should().Be(IntegerParseResult.NoNumber);
        }

        [Theory]
        [MemberData(nameof(InvalidNumberData))]
        public static void InvalidIn64Utf16(string invalidNumber)
        {
            var span = invalidNumber.AsSpan();

            var result = span.TryParseInt32(out _, out _);

            result.Should().Be(IntegerParseResult.NoNumber);
        }

        public static readonly TheoryData<string> InvalidNumberData =
            new TheoryData<string>
            {
                "-",
                "+",
                "k",
                "%!$",
                ""
            };

        [Theory]
        [MemberData(nameof(LeadingZeroesData))]
        public static void LeadingZeroesUtf8(string serializedNumber, int expectedNumber) =>
            TestValidNumberUtf8(serializedNumber, expectedNumber);

        [Theory]
        [MemberData(nameof(LeadingZeroesData))]
        public static void LeadingZeroesUtf16(string serializedNumber, int expectedNumber) =>
            TestValidNumberUtf16(serializedNumber, expectedNumber);

        public static readonly TheoryData<string, int> LeadingZeroesData =
            new TheoryData<string, int>
            {
                { "00", 0 },
                { "000000", 0 },
                { "00000140105", 140105 },
                { "-00000000001032", -1032 },
                { "000000002147483647", int.MaxValue },
                { "-000002147483648", int.MinValue }
            };

        [Theory]
        [MemberData(nameof(OverflowData))]
        public static void OverflowUtf8(string serializedNumber)
        {
            var span = new ReadOnlySpan<byte>(serializedNumber.ToUtf8());

            var result = span.TryParseInt32(out _, out _);

            result.Should().Be(IntegerParseResult.Overflow);
        }

        [Theory]
        [MemberData(nameof(OverflowData))]
        public static void OverflowUtf16(string serializedNumber)
        {
            var span = serializedNumber.AsSpan();

            var result = span.TryParseInt32(out _, out _);

            result.Should().Be(IntegerParseResult.Overflow);
        }

        public static readonly TheoryData<string> OverflowData =
            new TheoryData<string>
            {
                "2147483648",
                "-2147483649",
                "129223372036854775807",
                "-3921219223372036854775807"
            };

        private static void TestValidNumberUtf8(string serializedNumber, int expectedNumber)
        {
            var span = new ReadOnlySpan<byte>(serializedNumber.ToUtf8());

            var result = span.TryParseInt32(out var parsedNumber, out var bytesConsumed);

            result.Should().Be(IntegerParseResult.ParsingSuccessful);
            parsedNumber.Should().Be(expectedNumber);
            bytesConsumed.Should().Be(span.Length);
        }

        private static void TestValidNumberUtf16(string serializedNumber, int expectedNumber)
        {
            var span = serializedNumber.AsSpan();

            var result = span.TryParseInt32(out var parsedNumber, out var bytesConsumed);

            result.Should().Be(IntegerParseResult.ParsingSuccessful);
            parsedNumber.Should().Be(expectedNumber);
            bytesConsumed.Should().Be(span.Length);
        }
    }
}