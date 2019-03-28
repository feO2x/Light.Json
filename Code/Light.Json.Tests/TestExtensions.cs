using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using Xunit.Sdk;

namespace Light.Json.Tests
{
    public static class TestExtensions
    {
        public static void MustEqualSpan(in this ReadOnlySequence<char> sequence, ReadOnlySpan<char> span)
        {
            if (sequence.IsSingleSegment)
            {
                if (!sequence.First.Span.Equals(span, StringComparison.Ordinal))
                    ThrowSequenceNotEqualToSpanException(sequence, span);

                return;
            }

            var enumerator = new ReadOnlySequenceItemEnumerator<char>(sequence);
            char character;
            for (var i = 0; i < span.Length; ++i)
            {
                if (!enumerator.TryGetNext(out character) || character != span[i])
                    ThrowSequenceNotEqualToSpanException(sequence, span);
            }

            if (enumerator.TryGetNext(out character))
                ThrowSequenceNotEqualToSpanException(sequence, span);
        }

        private static void ThrowSequenceNotEqualToSpanException(in this ReadOnlySequence<char> sequence, ReadOnlySpan<char> span) =>
            throw new XunitException($"Expected \"{sequence.ToString()}\" to be equal to \"{span.ToString()}\", but it is not.");

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