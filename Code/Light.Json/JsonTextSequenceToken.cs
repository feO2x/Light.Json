using System;
using System.Runtime.CompilerServices;
using Light.Json.Streaming;

namespace Light.Json
{
    public readonly ref struct JsonTextSequenceToken
    {
        public readonly JsonTokenType Type;
        public readonly TextBufferSequence TextBufferSequence;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JsonTextSequenceToken(JsonTokenType type, TextBufferSequence sequence)
        {
            Type = type;
            TextBufferSequence = sequence;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(in JsonTextSequenceToken other) =>
            Type == other.Type && TextBufferSequence == other.TextBufferSequence;

        public override bool Equals(object obj) =>
            throw new NotSupportedException("ref structs do not support object.Equals as they cannot live on the heap.");

        public override int GetHashCode() =>
            throw new NotSupportedException("ref structs do not support object.GetHashCode as they cannot live on the heap.");

        public static bool operator ==(JsonTextSequenceToken x, JsonTextSequenceToken y) => x.Equals(y);
        public static bool operator !=(JsonTextSequenceToken x, JsonTextSequenceToken y) => !x.Equals(y);
    }
}