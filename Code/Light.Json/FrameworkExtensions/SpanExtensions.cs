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
    }
}
