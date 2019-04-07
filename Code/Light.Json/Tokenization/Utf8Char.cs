using System;
using System.Runtime.CompilerServices;
using System.Text;
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
        public static Utf8ParseResult TryParseNext(in ReadOnlySpan<byte> source, out Utf8Char character, int startIndex = 0)
        {
            // Most of the time, we expect single-byte UTF-8 characters.
            // Thus, we only perform a single check in this method and
            // inline it at the call site.
            if ((source[startIndex] & 0b1000_0000) == 0)
            {
                character = new Utf8Char(source.Slice(startIndex, 1));
                return Utf8ParseResult.CharacterParsedSuccessfully;
            }

            // If it is no single-byte sequence then we call another method
            // that performs the rest of the parsing.
            return TryParseMultiByteCharacter(source, out character, startIndex);
        }

        private static Utf8ParseResult TryParseMultiByteCharacter(in ReadOnlySpan<byte> source, out Utf8Char character, int startIndex)
        {
            var firstByte = source[startIndex];

            // If it starts with 110, then it's a two-byte UTF-8 character
            if ((firstByte & 0b1110_0000) == 0b1100_0000)
                return TrySlice(source, out character, startIndex, 2);

            // If it starts with 1110, then it's a three-byte UTF-8 character
            if ((firstByte & 0b1111_0000) == 0b1110_0000)
                return TrySlice(source, out character, startIndex, 3);

            // If it starts with 11110, then it's a four-byte UTF-8 character
            if ((firstByte & 0b1111_1000) == 0b1111_0000)
                return TrySlice(source, out character, startIndex, 4);

            character = default;

            // If it starts with 10, then the start index is invalid
            // (it does not point to the first byte of a UTF-8 multi-byte character)
            if ((firstByte & 0b1100_0000) == 0b1000_0000)
                return Utf8ParseResult.InvalidStartIndex;

            // Else the first byte is not compliant to the UTF-8 standard
            return Utf8ParseResult.InvalidFirstByte;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Utf8ParseResult TrySlice(in ReadOnlySpan<byte> source, out Utf8Char character, int startIndex, int length)
        {
            if (source.Length < startIndex + length)
            {
                character = default;
                return Utf8ParseResult.InsufficientBytes;
            }

            character = new Utf8Char(source.Slice(startIndex, length));
            return Utf8ParseResult.CharacterParsedSuccessfully;
        }

        public override string ToString()
        {
            if (Span.Length == 1)
                return new string((char) Span[0], 1);
            if (Span.Length == 0)
                return "";

            var bytes = new byte[Span.Length];
            Span.CopyTo(bytes.AsSpan());
            return Encoding.UTF8.GetString(bytes);
        }
    }
}