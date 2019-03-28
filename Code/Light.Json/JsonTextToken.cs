using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Light.Json
{
    public readonly ref struct JsonTextToken
    {
        public readonly JsonTokenType Type;
        public readonly ReadOnlySequence<char> Text;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JsonTextToken(JsonTokenType type, ReadOnlyMemory<char> text) 
            : this(type, text.ToSequence()) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JsonTextToken(JsonTokenType type, ReadOnlySequence<char> text = default)
        {
            Type = type;
            Text = text;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(in JsonTextToken other)
        {
            if (Type != other.Type)
                return false;

            if (Text.IsSingleSegment && other.Text.IsSingleSegment)
            {
                var span1 = Text.First.Span;
                var span2 = other.Text.First.Span;
                return span1 == span2 ||
                       span1.Equals(span2, StringComparison.Ordinal);
            }

            return EqualsOtherTokenSlow(other);
        }

        private bool EqualsOtherTokenSlow(in JsonTextToken other)
        {
            var enumeratorForMe = new ReadOnlySequenceItemEnumerator<char>(Text);
            var enumeratorForOther = new ReadOnlySequenceItemEnumerator<char>(other.Text);

            bool hasFoundMyCharacter;
            bool hasFoundOtherCharacter;

            while ((hasFoundMyCharacter = enumeratorForMe.TryGetNext(out var myCharacter)) & 
                   (hasFoundOtherCharacter = enumeratorForOther.TryGetNext(out var otherCharacter)))
            {
                if (myCharacter != otherCharacter)
                    return false;
            }

            return hasFoundMyCharacter == hasFoundOtherCharacter;
        }

        public override bool Equals(object obj) =>
            throw new NotSupportedException("ref structs do not support object.Equals as they cannot live on the heap.");

        public override int GetHashCode() =>
            throw new NotSupportedException("ref structs do not support object.GetHashCode as they cannot live on the heap.");

        public static bool operator ==(JsonTextToken x, JsonTextToken y) => x.Equals(y);
        public static bool operator !=(JsonTextToken x, JsonTextToken y) => !x.Equals(y);
    }
}