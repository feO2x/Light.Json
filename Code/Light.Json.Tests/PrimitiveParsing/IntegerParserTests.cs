using System;
using FluentAssertions;
using Light.Json.PrimitiveParsing;
using Light.Json.Tokenization.Utf8;
using Xunit;

namespace Light.Json.Tests.PrimitiveParsing
{
    public static class IntegerParserTests
    {
        [Theory]
        [MemberData(nameof(ValidInt64Data))]
        public static void ValidInt64Utf8(long number) =>
            TestValidNumberUtf8(number.ToString(), number);

        [Theory]
        [MemberData(nameof(ValidInt64Data))]
        public static void ValidInt64Utf16(long number) =>
            TestValidNumberUtf16(number.ToString(), number);

        public static readonly TheoryData<long> ValidInt64Data =
            new TheoryData<long>
            {
                0L,
                long.MinValue,
                long.MaxValue,
                long.MinValue / 10,
                long.MinValue + 1,
                long.MaxValue / 10,
                long.MaxValue - 1
            };

        [Theory]
        [MemberData(nameof(InvalidNumberData))]
        public static void InvalidInt64Utf8(string invalidNumber)
        {
            var span = new ReadOnlySpan<byte>(invalidNumber.ToUtf8());

            var result = span.TryParseInt64(out _, out _);

            result.Should().Be(IntegerParseResult.NoNumber);
        }

        [Theory]
        [MemberData(nameof(InvalidNumberData))]
        public static void InvalidIn64Utf16(string invalidNumber)
        {
            var span = invalidNumber.AsSpan();

            var result = span.TryParseInt64(out _, out _);

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
        public static void LeadingZeroesUtf8(string serializedNumber, long expectedNumber) =>
            TestValidNumberUtf8(serializedNumber, expectedNumber);

        [Theory]
        [MemberData(nameof(LeadingZeroesData))]
        public static void LeadingZeroesUtf16(string serializedNumber, long expectedNumber) =>
            TestValidNumberUtf16(serializedNumber, expectedNumber);

        public static readonly TheoryData<string, long> LeadingZeroesData =
            new TheoryData<string, long>
            {
                { "00", 0L },
                { "000000", 0L },
                { "00000140105", 140105L },
                { "-00000000001032", -1032L },
                { "000000009223372036854775807", long.MaxValue },
                { "-000009223372036854775808", long.MinValue }
            };

        [Theory]
        [MemberData(nameof(DecimalZeroesData))]
        public static void DecimalZeroesUtf8(string serializedNumber, long expectedNumber) =>
            TestValidNumberUtf8(serializedNumber, expectedNumber);

        [Theory]
        [MemberData(nameof(DecimalZeroesData))]
        public static void DecimalZeroesUtf16(string serializedNumber, long expectedNumber) =>
            TestValidNumberUtf16(serializedNumber, expectedNumber);

        public static readonly TheoryData<string, long> DecimalZeroesData =
            new TheoryData<string, long>
            {
                { "1.0", 1L },
                { "00042.000", 42L },
                { "192020192941341.000000000", 192020192941341L },
                { "9223372036854775807.000", long.MaxValue }
            };

        [Theory]
        [MemberData(nameof(OverflowData))]
        public static void OverflowUtf8(string serializedNumber)
        {
            var span = new ReadOnlySpan<byte>(serializedNumber.ToUtf8());

            var result = span.TryParseInt64(out _, out _);

            result.Should().Be(IntegerParseResult.Overflow);
        }

        [Theory]
        [MemberData(nameof(OverflowData))]
        public static void OverflowUtf16(string serializedNumber)
        {
            var span = serializedNumber.AsSpan();

            var result = span.TryParseInt64(out _, out _);

            result.Should().Be(IntegerParseResult.Overflow);
        }

        public static readonly TheoryData<string> OverflowData =
            new TheoryData<string>
            {
                "9223372036854775808",
                "-9223372036854775809",
                "129223372036854775807",
                "-3921219223372036854775807"
            };

        private static void TestValidNumberUtf8(string serializedNumber, long expectedNumber)
        {
            var span = new ReadOnlySpan<byte>(serializedNumber.ToUtf8());

            var result = span.TryParseInt64(out var parsedNumber, out var bytesConsumed);

            result.Should().Be(IntegerParseResult.ParsingSuccessful);
            parsedNumber.Should().Be(expectedNumber);
            bytesConsumed.Should().Be(span.Length);
        }

        private static void TestValidNumberUtf16(string serializedNumber, long expectedNumber)
        {
            var span = serializedNumber.AsSpan();

            var result = span.TryParseInt64(out var parsedNumber, out var bytesConsumed);

            result.Should().Be(IntegerParseResult.ParsingSuccessful);
            parsedNumber.Should().Be(expectedNumber);
            bytesConsumed.Should().Be(span.Length);
        }
    }
}