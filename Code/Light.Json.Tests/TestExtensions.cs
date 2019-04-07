using System;
using System.Runtime.CompilerServices;
using Xunit.Sdk;

namespace Light.Json.Tests
{
    public static class TestExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MustEqual(in this ReadOnlySpan<char> span, in ReadOnlySpan<char> other)
        {
            if (!span.Equals(other, StringComparison.Ordinal))
                ThrowTextSpansNotEqualException(span, other);
        }

        private static void ThrowTextSpansNotEqualException(in this ReadOnlySpan<char> span, in ReadOnlySpan<char> other) =>
            throw new XunitException($"Expected \"{span.ToString()}\" to be equal to \"{other.ToString()}\", but it is not.");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MustEqual(in this ReadOnlySpan<byte> span, in ReadOnlySpan<byte> other)
        {
            if (!span.SequenceEqual(other))
                ThrowByteSpansNotEqualException(span, other);
        }

        private static void ThrowByteSpansNotEqualException(in ReadOnlySpan<byte> span, in ReadOnlySpan<byte> other) =>
            throw new XunitException($"Expected byte sequence \"{span.ToString()}\" to be equal to \"{other.ToString()}\", but it is not.");
    }
}