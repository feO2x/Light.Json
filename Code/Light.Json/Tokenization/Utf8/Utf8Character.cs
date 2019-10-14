using System;
using Light.Json.FrameworkExtensions;

namespace Light.Json.Tokenization.Utf8
{
    public readonly ref struct Utf8Character
    {
        private Utf8Character(ReadOnlySpan<byte> span, int utf16Code)
        {
            Span = span;
            Utf16Code = utf16Code;
        }

        public ReadOnlySpan<byte> Span { get; }

        public int Utf16Code { get; }

        public int ByteLength => Span.Length;

        public bool IsSingleChar => Utf16Code <= char.MaxValue;
        public int NumberOfChars => Span.Length == 0 ? 0 : IsSingleChar ? 1 : 2;

        public bool Equals(in Utf8Character other) => Span.SequenceEqual(other.Span);

        public bool Equals(byte other) => Span.Length == 1 && Span[0] == other;

        public bool Equals(char other)
        {
            return Utf16Code == other;
        }

        public int CopyUtf16To(in Span<char> target, int startIndex)
        {
            if (startIndex < 0 || startIndex >= target.Length)
                return 0;

            if (IsSingleChar)
            {
                target[startIndex] = (char) Utf16Code;
                return 1;
            }

            if (startIndex + 1 == target.Length)
                return 0;

            target[startIndex] = (char) (Utf16Code >> 16);
            target[startIndex + 1] = (char) (Utf16Code & 0xFFFF);
            return 2;
        }

        public override bool Equals(object obj) =>
            throw BoxingNotSupported.CreateException();

        public override int GetHashCode() =>
            throw BoxingNotSupported.CreateException();

        public static Utf8ParseResult TryParseNext(in ReadOnlySpan<byte> source, out Utf8Character character, int startIndex = 0)
        {
            if (startIndex < 0 || startIndex >= source.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            // Most of the time, we expect single-byte UTF-8 characters.
            // Thus, we only perform a single check in this method and
            // inline it at the call site.
            if ((source[startIndex] & 0b1000_0000) == 0)
            {
                var span = source.Slice(startIndex, 1);
                var utf16Code = span[0];
                character = new Utf8Character(span, utf16Code);
                return Utf8ParseResult.CharacterParsedSuccessfully;
            }

            // If it is no single-byte sequence then we call another method
            // that performs the rest of the parsing.
            return TryParseMultiByteCharacter(source, out character, startIndex);
        }

        private static Utf8ParseResult TryParseMultiByteCharacter(in ReadOnlySpan<byte> source, out Utf8Character character, int startIndex)
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

            // Else the first byte is not compliant to the UTF-8 standard
            character = default;
            return Utf8ParseResult.ByteSequenceIsNotUtf8Compliant;
        }

        private static Utf8ParseResult TrySlice(in ReadOnlySpan<byte> source, out Utf8Character character, int startIndex, int length)
        {
            if (source.Length < startIndex + length)
            {
                character = default;
                return Utf8ParseResult.InsufficientBytes;
            }

            var span = source.Slice(startIndex, length);
            int utf16Code;
            switch (length)
            {
                case 2:
                    utf16Code = (span[0] & 0b0001_1111) << 6; // Last 5 bits of first byte, moved to the left by 6 bits
                    utf16Code |= span[1] & 0b0011_1111; // Last 6 bits of the second byte
                    break;
                case 3:
                    utf16Code = (span[0] & 0b0000_1111) << 12; // Last 4 bits of the first byte, moved to the left by 12 bits
                    utf16Code |= (span[1] & 0b0011_1111) << 6; // Last 6 bits of the second byte, moved to the left by 6 bits
                    utf16Code |= span[2] & 0b0011_1111; // Last 6 bits of the third byte
                    break;
                default: // in this case, length is always 4
                    utf16Code = (span[0] & 0b0000_0111) << 18; // Last 3 bits of the first byte, moved to the left by 18 bits
                    utf16Code |= (span[1] & 0b0011_1111) << 12; // Last 6 bits of the seconds byte, moved to the left by 12 bits
                    utf16Code |= (span[2] & 0b0011_1111) << 6; // Last 6 bits of the third byte, moved to the left by 6 bits
                    utf16Code |= (span[3] & 0b0011_1111); // Last 6 bits of the fourth byte
                    break;
            }

            character = new Utf8Character(span, utf16Code);
            return Utf8ParseResult.CharacterParsedSuccessfully;
        }

        public override string ToString()
        {
            if (Span.Length == 1)
                return new string((char) Utf16Code, 1);
            if (Span.Length == 0)
                return "";

            Span<char> span = stackalloc char[]
            {
                (char) (Utf16Code >> 16),
                (char) (Utf16Code & 0xFFFF)
            };
            return span.ToString();
        }

        public static bool operator ==(in Utf8Character x, in Utf8Character y) => x.Equals(y);

        public static bool operator !=(in Utf8Character x, in Utf8Character y) => !x.Equals(y);

        public static bool operator ==(in Utf8Character x, char y) => x.Equals(y);
        public static bool operator !=(in Utf8Character x, char y) => !x.Equals(y);

        public static bool operator ==(char x, in Utf8Character y) => y.Equals(x);
        public static bool operator !=(char x, in Utf8Character y) => !y.Equals(x);

        public static bool operator ==(in Utf8Character x, byte y) => x.Equals(y);
        public static bool operator !=(in Utf8Character x, byte y) => !x.Equals(y);

        public static bool operator ==(byte x, in Utf8Character y) => y.Equals(x);
        public static bool operator !=(byte x, in Utf8Character y) => !y.Equals(x);
    }
}