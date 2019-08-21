using System;
using System.Runtime.CompilerServices;
using Light.Json.FrameworkExtensions;

namespace Light.Json.Tokenization.Utf8
{
    public readonly ref struct JsonUtf8Token
    {
        public readonly JsonTokenType Type;
        public readonly ReadOnlySpan<byte> ByteSequence;
        public readonly int NumberOfCharacters;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JsonUtf8Token(JsonTokenType type, ReadOnlySpan<byte> byteSequence, int numberOfCharacters)
        {
            Type = type;
            ByteSequence = byteSequence;
            NumberOfCharacters = numberOfCharacters;
        }

        public int ByteLength
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ByteSequence.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(JsonUtf8Token other) =>
            Type == other.Type && 
            NumberOfCharacters == other.NumberOfCharacters && 
            (ByteSequence == other.ByteSequence || ByteSequence.SequenceEqual(other.ByteSequence));

        public override bool Equals(object obj) =>
            throw BoxingNotSupported.CreateException();

        public override int GetHashCode() =>
            throw BoxingNotSupported.CreateException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(JsonUtf8Token x, JsonUtf8Token y) => x.Equals(y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(JsonUtf8Token x, JsonUtf8Token y) => !x.Equals(y);
    }
}