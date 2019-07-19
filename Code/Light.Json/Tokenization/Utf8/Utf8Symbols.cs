using System;

namespace Light.Json.Tokenization.Utf8
{
    public static class Utf8Symbols
    {
        private static readonly byte[] InternalArray;
        public static readonly ReadOnlyMemory<byte> False;
        public static readonly ReadOnlyMemory<byte> True;
        public static readonly ReadOnlyMemory<byte> Null;

        static Utf8Symbols()
        {
            InternalArray = new byte[13];
            var currentIndex = 0;

            // False is at index 0 to 4
            False = JsonSymbols.False.ToUtf8().CopyToInternalArray(ref currentIndex);
            // True is at index 5 to 8
            True = JsonSymbols.True.ToUtf8().CopyToInternalArray(ref currentIndex);
            // Null is at index 9 to 12
            Null = JsonSymbols.Null.ToUtf8().CopyToInternalArray(ref currentIndex);
        }

        private static ReadOnlyMemory<byte> CopyToInternalArray(this byte[] utf8Symbol, ref int startIndex)
        {
            for (var i = 0; i < utf8Symbol.Length; ++i)
            {
                InternalArray[i + startIndex] = utf8Symbol[i];
            }

            var memory = new ReadOnlyMemory<byte>(InternalArray, startIndex, utf8Symbol.Length);
            startIndex += utf8Symbol.Length;
            return memory;
        }
    }
}