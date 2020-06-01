using System.Buffers;
using Light.GuardClauses;

namespace Light.Json.Buffers
{
    public sealed class ArrayPoolBufferProvider<T> : IBufferProvider<T>
    {
        public ArrayPoolBufferProvider(ArrayPool<T>? arrayPool = null,
                                       int initialBufferSizeInByte = 262_144,
                                       int? maximumBufferSizeInByte = null,
                                       IIncreaseBufferSizeStrategy? increaseBufferSizeStrategy = null)
        {
            MaximumBufferSizeInByte = maximumBufferSizeInByte?.MustBeGreaterThanOrEqualTo(initialBufferSizeInByte) ?? MaximumArraySize.GetMaximumArraySizeInByte<T>();
            ArrayPool = arrayPool ?? ArrayPool<T>.Shared;
            InitialBufferSizeInByte = initialBufferSizeInByte.MustBeIn(Range.FromInclusive(1).ToInclusive(MaximumBufferSizeInByte));
            IncreaseBufferSizeStrategy = increaseBufferSizeStrategy ?? new MultiplyBufferSizeStrategy();
        }

        public ArrayPool<T> ArrayPool { get; }
        public int InitialBufferSizeInByte { get; }
        public int MaximumBufferSizeInByte { get; }
        public IIncreaseBufferSizeStrategy IncreaseBufferSizeStrategy { get; }

        public T[] GetInitialBuffer() => ArrayPool.Rent(InitialBufferSizeInByte);

        public T[] GetNewBufferWithIncreasedSize(T[] currentBuffer, int numberOfAdditionalSlots)
        {
            var newSizeInByte = IncreaseBufferSizeStrategy.DetermineNewBufferSize(currentBuffer.Length, numberOfAdditionalSlots, MaximumBufferSizeInByte);
            var newBuffer = ArrayPool.Rent(newSizeInByte);
            currentBuffer.CopyTo(newBuffer, 0);
            ArrayPool.Return(currentBuffer);
            return newBuffer;
        }

        public void Finish(T[] currentBuffer) => ArrayPool.Return(currentBuffer);
    }
}