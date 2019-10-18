using System;
using Light.Json.Tokenization.Utf8;
using Xunit.Sdk;

namespace Light.Json.Tests
{
    public static class TestExtensions
    {
        public static void MustEqual(in this ReadOnlySpan<char> span, in ReadOnlySpan<char> other)
        {
            if (!span.Equals(other, StringComparison.Ordinal))
                ThrowTextSpansNotEqualException(span, other);
        }

        private static void ThrowTextSpansNotEqualException(in this ReadOnlySpan<char> span, in ReadOnlySpan<char> other) =>
            throw new XunitException($"Expected \"{span.ToString()}\" to be equal to \"{other.ToString()}\", but it is not.");

        public static void MustEqual(in this ReadOnlySpan<byte> span, in ReadOnlySpan<byte> other)
        {
            if (!span.SequenceEqual(other))
                ThrowByteSpansNotEqualException(span, other);
        }

        private static void ThrowByteSpansNotEqualException(in ReadOnlySpan<byte> span, in ReadOnlySpan<byte> other) =>
            throw new XunitException($"Expected byte sequence \"{span.ToString()}\" to be equal to \"{other.ToString()}\", but it is not.");

        public static void MustBeDefault(this in Utf8Character character)
        {
            if (!character.Equals(default(Utf8Character)))
                ThrowValueNotDefault(character);
        }

        private static void ThrowValueNotDefault(in Utf8Character character) =>
            throw new XunitException($"Expected  \"{character.ToString()}\" to be the default value, but it is not.");
    }
}