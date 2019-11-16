using System;
using System.Collections.Generic;
using System.Text;

namespace Light.Json.FrameworkExtensions
{
    public static class SpanExtensions
    {
        public static unsafe string ConvertFromUtf8ToString(this ReadOnlySpan<byte> utf8Span)
        {
            fixed (byte* bytePointer = utf8Span)
                return Encoding.UTF8.GetString(bytePointer, utf8Span.Length);
        }

        public static unsafe string ConvertFromUtf8ToString(this ReadOnlySpan<byte> utf8Span, int length)
        {
            fixed (byte* bytePointer = utf8Span)
                return Encoding.UTF8.GetString(bytePointer, length);
        }

        public static unsafe string ConvertFromUtf8ToString(this Span<byte> utf8Span, int length)
        {
            fixed (byte* bytePointer = utf8Span)
                return Encoding.UTF8.GetString(bytePointer, length);
        }

        public static void AppendAsciiCharacter(this Span<byte> utf8Span, char utf16Character, ref int currentIndex)
        {
            utf8Span[currentIndex++] = (byte) utf16Character;
        }

        public static int AppendEscapeSequence(this Span<byte> utf8Span, ReadOnlySpan<byte> source, int sourceStartIndex)
        {
            throw new NotImplementedException();
        }
    }
}
