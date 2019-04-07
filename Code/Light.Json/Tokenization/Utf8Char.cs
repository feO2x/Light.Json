using System;
using System.Runtime.CompilerServices;
using Light.Json.FrameworkExtensions;

namespace Light.Json.Tokenization
{
    public readonly ref struct Utf8Char
    {
        public readonly ReadOnlySpan<byte> Span;

        private Utf8Char(ReadOnlySpan<byte> span) =>
            Span = span;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(in Utf8Char other) =>
            Span.SequenceEqual(other.Span);

        public override bool Equals(object obj) =>
            throw BoxingNotSupported.CreateException();

        public override int GetHashCode() =>
            throw BoxingNotSupported.CreateException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParseNext(in ReadOnlySpan<byte> source, out Utf8Char character, int startIndex = 0)
        {
            // Most of the time, we expect single-byte UTF-8 characters.
            // Thus, we only perform a single check in this method and
            // inline it at the call site.
            if ((source[startIndex] & 0b1000_0000) == 0)
            {
                character = new Utf8Char(source.Slice(startIndex, 1));
                return true;
            }

            // If it is no single-byte sequence then we call another method
            // that performs the rest of the parsing.
            return TryParseMultiByteCharacter(source, out character, startIndex);
        }

        private static bool TryParseMultiByteCharacter(in ReadOnlySpan<byte> source, out Utf8Char character, int startIndex)
        {
            var firstByte = source[startIndex];

            // If it starts with 110, then it's a two-byte UTF-8 character
            if ((firstByte & 0b1110_0000) == 0b1100_0000)
            {
                character = new Utf8Char(source.Slice(startIndex, 2));
                return true;
            }

            // If it starts with 1110, then it's a three-byte UTF-8 character
            if ((firstByte & 0b1111_0000) == 0b1110_0000)
            {
                character = new Utf8Char(source.Slice(startIndex, 3));
                return true;
            }

            // If it starts with 11110, then it's a four-byte UTF-8 character
            if ((firstByte & 0b1111_1000) == 0b1111_0000)
            {
                character = new Utf8Char(source.Slice(startIndex, 4));
                return true;
            }

            character = default;
            return false;
        }
    }
}