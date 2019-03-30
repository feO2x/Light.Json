using System;
using System.Runtime.CompilerServices;
using Light.GuardClauses;
using Light.Json.FrameworkExtensions;

namespace Light.Json.Streaming
{
    public readonly ref struct TextBufferSequence
    {
        public readonly ReadOnlySpan<TextBuffer> Buffers;
        public readonly int StartIndexOfFirstBuffer;
        public readonly int EndIndexOfLastBuffer;
        public readonly int CharacterLength;

        public TextBufferSequence(in ReadOnlySpan<TextBuffer> buffers, int startIndexOfFirstBuffer, int endIndexOfLastBuffer)
        {
            Buffers = buffers;
            StartIndexOfFirstBuffer = startIndexOfFirstBuffer;
            EndIndexOfLastBuffer = endIndexOfLastBuffer;

            if (buffers.IsEmpty)
            {
                CharacterLength = 0;
                return;
            }

            startIndexOfFirstBuffer.MustNotBeLessThan(0, nameof(startIndexOfFirstBuffer))
                                   .MustNotBeGreaterThanOrEqualTo(buffers[0].Length, nameof(startIndexOfFirstBuffer));
            endIndexOfLastBuffer.MustNotBeLessThan(0, nameof(endIndexOfLastBuffer))
                                .MustNotBeGreaterThanOrEqualTo(buffers[buffers.Length - 1].Length, nameof(endIndexOfLastBuffer));

            if (buffers.Length == 1)
            {
                CharacterLength = buffers[0].Slice(startIndexOfFirstBuffer, endIndexOfLastBuffer).Length;
                return;
            }

            CharacterLength = buffers[0].Slice(startIndexOfFirstBuffer).Length;
            for (var i = 1; i < buffers.Length - 1; ++i)
            {
                CharacterLength += buffers[i].Length;
            }

            CharacterLength += buffers[buffers.Length - 1].Slice(0, endIndexOfLastBuffer).Length;
        }

        public ref readonly TextBuffer this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref Buffers[index];
        }

        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Buffers.IsEmpty;
        }

        public int BufferCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Buffers.Length;
        }

        public bool TryGetSingleSpan(out ReadOnlySpan<char> text)
        {
            if (Buffers.Length != 1)
            {
                text = default;
                return false;
            }

            text = Buffers[0].Slice(StartIndexOfFirstBuffer, EndIndexOfLastBuffer);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(in TextBufferSequence other)
        {
            if (StartIndexOfFirstBuffer != other.StartIndexOfFirstBuffer ||
                EndIndexOfLastBuffer != other.EndIndexOfLastBuffer ||
                CharacterLength != other.CharacterLength)
            {
                return false;
            }

            if (Buffers.Length == 1 &&
                other.Buffers.Length == 1)
            {
                var mySpan = Buffers[0].Slice(StartIndexOfFirstBuffer, EndIndexOfLastBuffer);
                var otherSpan = other.Buffers[0].Slice(StartIndexOfFirstBuffer, EndIndexOfLastBuffer);
                return mySpan.Equals(otherSpan, StringComparison.Ordinal);
            }

            return EqualsSlow(other);
        }

        private bool EqualsSlow(in TextBufferSequence other)
        {
            var myEnumerator = new TextBufferSequenceEnumerator(this);
            var otherEnumerator = new TextBufferSequenceEnumerator(other);

            char myCharacter;
            char otherCharacter;
            while (myEnumerator.TryGetNext(out myCharacter) &
                   otherEnumerator.TryGetNext(out otherCharacter))
            {
                if (myCharacter != otherCharacter)
                    return false;
            }

            return myEnumerator.TryGetNext(out myCharacter) != otherEnumerator.TryGetNext(out otherCharacter);
        }

        public override bool Equals(object obj) => throw BoxingNotSupported.CreateException();

        public override int GetHashCode() => throw BoxingNotSupported.CreateException();

        public static bool operator ==(in TextBufferSequence x, in TextBufferSequence y) => x.Equals(y);
        public static bool operator !=(in TextBufferSequence x, in TextBufferSequence y) => !x.Equals(y);
    }
}