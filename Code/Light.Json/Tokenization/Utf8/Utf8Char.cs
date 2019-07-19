using System;
using System.Runtime.CompilerServices;
using System.Text;
using Light.Json.FrameworkExtensions;

namespace Light.Json.Tokenization.Utf8
{
    public readonly ref struct Utf8Char
    {
        public readonly ReadOnlySpan<byte> Span;

        private Utf8Char(in ReadOnlySpan<byte> span) =>
            Span = span;

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Span.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(in Utf8Char other) =>
            Span.SequenceEqual(other.Span);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(in ReadOnlySpan<byte> other) =>
            Span.SequenceEqual(other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(byte other) => Span.Length == 1 && Span[0] == other;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(char other)
        {
            if (Span.Length == 1)
                return (char) Span[0] == other;

            return EqualsSlow(other);
        }

        private bool EqualsSlow(char other)
        {
            if (Span.Length == 0 || Span.Length == 4)
                return false;

            // If it's a two-byte or three-byte UTF-8 char, then we will convert the 
            // unicode bits to an int value and compare it to the character.
            if (TryConvertTwoByteCharacter(out var value) || TryConvertThreeByteCharacter(out value))
                return value == other;

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryConvertTwoByteCharacter(out int utf16Result)
        {
            if (Span.Length != 2)
            {
                utf16Result = default;
                return false;
            }

            utf16Result = (Span[0] & 0b0001_1111) << 6; // Last 5 bits of first byte, moved to the left by 6 bits
            utf16Result |= Span[1] & 0b0011_1111; // Last 6 bits of the second byte
            return true;
        }

        public bool TryConvertThreeByteCharacter(out int utf16Result)
        {
            if (Span.Length != 3)
            {
                utf16Result = default;
                return false;
            }

            utf16Result = (Span[0] & 0b0000_1111) << 12; // Last 4 bits of the first byte, moved to the left by 12 bits
            utf16Result |= (Span[1] & 0b0011_1111) << 6; // Last 6 bits of the second byte, moved to the left by 6 bits
            utf16Result |= Span[2] & 0b0011_1111; // Last 6 bits of the third byte
            return true;
        }

        public override bool Equals(object obj) =>
            throw BoxingNotSupported.CreateException();

        public override int GetHashCode() =>
            throw BoxingNotSupported.CreateException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Utf8ParseResult TryParseNext(in ReadOnlySpan<byte> source, out Utf8Char character, int startIndex = 0)
        {
            if (startIndex < 0 || startIndex >= source.Length)
            {
                character = default;
                return Utf8ParseResult.InvalidStartIndex;
            }

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Utf8Char x, in Utf8Char y) => x.Equals(y);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Utf8Char x, in Utf8Char y) => !x.Equals(y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Utf8Char x, char y) => x.Equals(y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Utf8Char x, char y) => !x.Equals(y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(char x, in Utf8Char y) => y.Equals(x);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(char x, in Utf8Char y) => !y.Equals(x);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Utf8Char x, byte y) => x.Equals(y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Utf8Char x, byte y) => !x.Equals(y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(byte x, in Utf8Char y) => y.Equals(x);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(byte x, in Utf8Char y) => !y.Equals(x);
    }
}