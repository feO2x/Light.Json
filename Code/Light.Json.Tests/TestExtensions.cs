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
                ThrowSpanNotEqualToSpanException(span, other);
        }

        private static void ThrowSpanNotEqualToSpanException(in this ReadOnlySpan<char> span, ReadOnlySpan<char> other) =>
            throw new XunitException($"Expected \"{span.ToString()}\" to be equal to \"{other.ToString()}\", but it is not.");
    }
}