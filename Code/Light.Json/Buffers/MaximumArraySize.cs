using System;

namespace Light.Json.Buffers
{
    public static class MaximumArraySize
    {
        public const int Byte64Bit = 2_147_483_591;
        public const int Char64Bit = 1_073_741_795;
        public const int Byte32Bit = 2_130_702_268;
        public const int Char32Bit = 1_065_351_134;

        public static int GetMaximumByteArraySize() => Environment.Is64BitProcess ? Byte64Bit : Byte32Bit;

        public static int GetMaximumCharArraySize() => Environment.Is64BitProcess ? Char64Bit : Char32Bit;
    }
}