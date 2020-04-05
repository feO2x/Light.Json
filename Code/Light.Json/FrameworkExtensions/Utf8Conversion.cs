using System;
using System.Text;

namespace Light.Json.FrameworkExtensions
{
    public static class Utf8Conversion
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

        public static byte[] ToUtf8(this char character) => Encoding.UTF8.GetBytes(character.ToString());

        public static byte[] ToUtf8(this string @string) => Encoding.UTF8.GetBytes(@string);
    }
}