using System.Buffers;
using System.Runtime.Serialization;
using Light.GuardClauses;

namespace Light.Json.Serialization.Buffers
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

        public T[] GetNewBufferWithIncreasedSize(T[] currentBuffer)
        {
            currentBuffer.MustNotBeNull(nameof(currentBuffer));

            var newSize = DetermineNewBufferSize(currentBuffer.Length);
            var newBuffer = new T[newSize];
            currentBuffer.CopyTo(newBuffer, 0);
            ArrayPool.Return(currentBuffer);
            return newBuffer;
        }

        public void Finish(T[] currentBuffer) => ArrayPool.Return(currentBuffer.MustNotBeNull(nameof(currentBuffer)));

        protected virtual int DetermineNewBufferSize(int currentSize)
        {
            if (_maximumArraySize <= currentSize)
                throw new SerializationException($"A new buffer for serialization cannot be created because the current size of {currentSize} is the maximum size.");
            var newSize = currentSize * 2;
            if (newSize > _maximumArraySize)
                newSize = _maximumArraySize;
            return newSize;
        }
    }
}