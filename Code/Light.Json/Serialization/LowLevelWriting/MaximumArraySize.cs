using System;

namespace Light.Json.Serialization.LowLevelWriting
{
    public static class MaximumArraySize
    {
        public const int Byte64Bit = 2_147_483_591;
        public const int Char64Bit = 1_073_741_795;
        public const int Byte32Bit = 2_130_702_268;
        public const int Char32Bit = 1_065_351_134;

        public static int GetMaximumArraySize<T>()
        {
            if (typeof(T) == typeof(char))
                return Environment.Is64BitProcess ? Char64Bit : Char32Bit;

            if (typeof(T) == typeof(byte))
                return Environment.Is64BitProcess ? Byte64Bit : Byte32Bit;

            throw new InvalidOperationException($"Only char and byte are currently supported, but you provided \"{typeof(T)}\".");
        }
    }
}