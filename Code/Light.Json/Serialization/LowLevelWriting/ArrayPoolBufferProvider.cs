using System.Buffers;
using System.Runtime.Serialization;
using Light.GuardClauses;

namespace Light.Json.Serialization.LowLevelWriting
{
    public class ArrayPoolBufferProvider<T> : IInMemoryBufferProvider<T>
    {
        private readonly int _maximumArraySize;
        public readonly ArrayPool<T> ArrayPool;
        public readonly int DefaultArraySize;

        public ArrayPoolBufferProvider(int defaultArraySize = 262_144, ArrayPool<T>? arrayPool = null)
        {
            _maximumArraySize = MaximumArraySize.GetMaximumArraySize<T>();
            DefaultArraySize = defaultArraySize.MustBeIn(Range.FromExclusive(0).ToInclusive(_maximumArraySize), nameof(defaultArraySize));
            ArrayPool = arrayPool ?? ArrayPool<T>.Shared;
        }

        public T[] GetInitialBuffer() => ArrayPool.Rent(DefaultArraySize);
        public T[] GetNewBufferWithIncreasedSize(T[] currentBuffer, int numberOfRequiredBufferSlots)
        {
            currentBuffer.MustNotBeNull(nameof(currentBuffer));
            numberOfRequiredBufferSlots.MustBeGreaterThan(0, nameof(numberOfRequiredBufferSlots));

            var newSize = DetermineNewBufferSize(currentBuffer.Length, numberOfRequiredBufferSlots);
            var newBuffer = ArrayPool.Rent(newSize);
            currentBuffer.CopyTo(newBuffer, 0);
            ArrayPool.Return(currentBuffer);
            return newBuffer;
        }

        public void Finish(T[] currentBuffer) => ArrayPool.Return(currentBuffer.MustNotBeNull(nameof(currentBuffer)));

        protected virtual int DetermineNewBufferSize(int currentSize, int numberOfRequiredBufferSlots)
        {
            var newMinimumSize = currentSize + numberOfRequiredBufferSlots;
            if (_maximumArraySize < newMinimumSize)
                throw new SerializationException($"A new buffer for in-memory serialization cannot be created because the required size of {newMinimumSize} is larger than the maximum buffer size.");

            var newSize = currentSize * 2;
            if (newSize < newMinimumSize)
                newSize = newMinimumSize;
            if (newSize > _maximumArraySize)
                newSize = _maximumArraySize;
            return newSize;
        }
    }
}