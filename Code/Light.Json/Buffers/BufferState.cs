using System;
using Light.GuardClauses;

namespace Light.Json.Buffers
{
    public readonly struct BufferState<T> : IEquatable<BufferState<T>>
    {
        public BufferState(T[] currentBuffer, int currentIndex, int ensuredIndex)
        {
            CurrentBuffer = currentBuffer.MustNotBeNull(nameof(currentBuffer));
            CurrentIndex = currentIndex.MustNotBeLessThan(0, nameof(currentIndex));
            EnsuredIndex = ensuredIndex.MustBeGreaterThanOrEqualTo(ensuredIndex);
        }

        public T[] CurrentBuffer { get; }

        public int CurrentIndex { get; }

        public int EnsuredIndex { get; }

        public bool Equals(BufferState<T> other) =>
            ReferenceEquals(CurrentBuffer, other.CurrentBuffer) &&
            CurrentIndex == other.CurrentIndex &&
            EnsuredIndex == other.EnsuredIndex;

        public override bool Equals(object? obj) =>
            obj is BufferState<T> other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = CurrentBuffer.GetHashCode();
                hashCode = (hashCode * 397) ^ CurrentIndex;
                hashCode = (hashCode * 397) ^ EnsuredIndex;
                return hashCode;
            }
        }
    }
}