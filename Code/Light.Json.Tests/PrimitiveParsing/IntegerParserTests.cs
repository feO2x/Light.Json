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
        [InlineData(0L)]
        [InlineData(long.MinValue)]
        [InlineData(long.MaxValue)]
        [InlineData(long.MinValue / 10)]
        [InlineData(long.MinValue + 1)]
        [InlineData(long.MaxValue / 10)]
        [InlineData(long.MaxValue - 1)]
        public static void ValidInt64(long number)
        {
            var serializedNumber = new ReadOnlySpan<byte>(number.ToString().ToUtf8());

            var result = serializedNumber.TryParseInt64(out var parsedNumber, out var bytesConsumed);

            result.Should().Be(IntegerParseResult.Success);
            parsedNumber.Should().Be(number);
            bytesConsumed.Should().Be(serializedNumber.Length);
        }

        [Theory]
        [InlineData("-")]
        [InlineData("+")]
        [InlineData("k")]
        [InlineData("%!$")]
        [InlineData("")]
        public static void InvalidInt64(string invalidNumber)
        {
            var span = new ReadOnlySpan<byte>(invalidNumber.ToUtf8());

            var result = span.TryParseInt64(out _, out _);

            result.Should().Be(IntegerParseResult.NoNumber);
        }

        [Theory]
        [InlineData("00", 0L)]
        [InlineData("000000", 0L)]
        [InlineData("00000140105", 140105L)]
        [InlineData("-00000000001032", -1032L)]
        [InlineData("000000009223372036854775807", long.MaxValue)]
        [InlineData("-000009223372036854775808", long.MinValue)]
        public static void LeadingZeroes(string serializedNumber, long expectedNumber)
        {
            var span = new ReadOnlySpan<byte>(serializedNumber.ToUtf8());

            var result = span.TryParseInt64(out var parsedNumber, out var bytesConsumed);

            result.Should().Be(IntegerParseResult.Success);
            parsedNumber.Should().Be(expectedNumber);
            bytesConsumed.Should().Be(span.Length);
        }

        [Theory]
        [InlineData("9223372036854775808")]
        [InlineData("-9223372036854775809")]
        [InlineData("129223372036854775807")]
        [InlineData("-3921219223372036854775807")]
        public static void Overflow(string serializedNumber)
        {
            var span = new ReadOnlySpan<byte>(serializedNumber.ToUtf8());

            var result = span.TryParseInt64(out _, out _);

            result.Should().Be(IntegerParseResult.Overflow);
        }
    }
}