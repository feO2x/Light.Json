using System;
using System.Runtime.CompilerServices;

namespace Light.Json.Tokenization.Utf8
{
    public static class Utf8Symbols
    {
        private static readonly byte[] InternalArray;
        public static readonly Utf8Constant False;
        public static readonly Utf8Constant True;
        public static readonly Utf8Constant Null;

        static Utf8Symbols()
        {
            InternalArray = new byte[13];
            var currentIndex = 0;

            // False is at index 0 to 4
            False = JsonSymbols.False.ConvertAndCopyToInternalArray(ref currentIndex);
            // True is at index 5 to 8
            True = JsonSymbols.True.ConvertAndCopyToInternalArray(ref currentIndex);
            // Null is at index 9 to 12
            Null = JsonSymbols.Null.ConvertAndCopyToInternalArray(ref currentIndex);
        }

        private static Utf8Constant ConvertAndCopyToInternalArray(this string utf16Constant, ref int startIndex)
        {
            var utf8Bytes = utf16Constant.ToUtf8();

            for (var i = 0; i < utf8Bytes.Length; ++i)
            {
                InternalArray[i + startIndex] = utf8Bytes[i];
            }

            var memory = new ReadOnlyMemory<byte>(InternalArray, startIndex, utf8Bytes.Length);
            startIndex += utf8Bytes.Length;
            return new Utf8Constant(memory, utf16Constant);
        }
    }

    public readonly struct Utf8Constant
    {
        public readonly ReadOnlyMemory<byte> ByteSequence;
        public readonly string Utf16Text;

        public Utf8Constant(ReadOnlyMemory<byte> byteSequence, string utf16Text)
        {
            ByteSequence = byteSequence;
            Utf16Text = utf16Text;
        }

        public int NumberOfCharacters
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Utf16Text.Length;
        }

        public int ByteLength
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ByteSequence.Length;
        }
    }
}