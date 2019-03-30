using System;

namespace Light.Json.FrameworkExtensions
{
    public static class BoxingNotSupported
    {
        public static NotSupportedException CreateException() =>
            new NotSupportedException("Ref structs cannot be boxed on the heap.");
    }
}