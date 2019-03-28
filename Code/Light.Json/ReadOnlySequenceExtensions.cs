using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Light.Json
{
    public static class ReadOnlySequenceExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySequence<T> ToSequence<T>(in this ReadOnlyMemory<T> memory) =>
            new ReadOnlySequence<T>(memory);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EqualsSpan(in this ReadOnlySequence<char> sequence, in ReadOnlySpan<char> span) =>
            sequence.IsSingleSegment ? sequence.First.Span.Equals(span, StringComparison.Ordinal) : sequence.EqualsSpanSlow(span);

        private static bool EqualsSpanSlow(in this ReadOnlySequence<char> sequence, in ReadOnlySpan<char> span)
        {
            var enumerator = new ReadOnlySequenceItemEnumerator<char>(sequence);
            char character;
            for (var i = 0; i < span.Length; ++i)
            {
                if (!enumerator.TryGetNext(out character) || character != span[i])
                    return false;
            }

            return !enumerator.TryGetNext(out character);
        }
    }
}