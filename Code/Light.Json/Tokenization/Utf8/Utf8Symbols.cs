using System;

namespace Light.Json.Tokenization.Utf8
{
    public static class Utf8Symbols
    {
        private static readonly byte[] InternalArray;

        static Utf8Symbols()
        {
            InternalArray = new byte[5];
            var currentIndex = 0;

            // False is at index 0 to 4
            False = JsonSymbols.False.ToUtf8().CopyToInternalArray(ref currentIndex);
        }

        public static ReadOnlyMemory<byte> False { get; }

        private static ReadOnlyMemory<byte> CopyToInternalArray(this byte[] utf8Symbol, ref int startIndex)
        {
            for (var i = 0; i < utf8Symbol.Length; ++i)
            {
                InternalArray[i + startIndex] = utf8Symbol[i];
            }

            return new ReadOnlyMemory<byte>(InternalArray, startIndex, utf8Symbol.Length);
        }
    }
}