using System.Buffers;
using System.Threading.Tasks;
using Light.GuardClauses;

namespace Light.Json.Buffers
{
    public sealed class InMemoryArrayPoolBufferWriterService<T> : BaseArrayPoolBufferWriterService<T>
    {
        public InMemoryArrayPoolBufferWriterService(ArrayPool<T>? arrayPool = null,
                                                    int initialArraySizeInByte = 262_144,
                                                    int? maximumArraySizeInByte = null,
                                                    IIncreaseBufferSizeStrategy? increaseBufferSizeStrategy = null)
            : base(arrayPool, initialArraySizeInByte, maximumArraySizeInByte, increaseBufferSizeStrategy) { }

        public override ValueTask<BufferState<T>> ProcessFullBufferAsync(BufferState<T> bufferState, int numberOfAdditionalSlots)
        {
            numberOfAdditionalSlots.MustBeGreaterThan(0, nameof(numberOfAdditionalSlots));

            var oldBuffer = bufferState.CurrentBuffer;
            var newSize = IncreaseBufferSizeStrategy.DetermineNewBufferSize(oldBuffer.Length, numberOfAdditionalSlots, MaximumBufferSizeInByte);
            var newBuffer = ArrayPool.Rent(newSize);
            oldBuffer.CopyTo(newBuffer, 0);
            ArrayPool.Return(oldBuffer);
            var newBufferState = new BufferState<T>(newBuffer, bufferState.CurrentIndex, bufferState.EnsuredIndex);
            return new ValueTask<BufferState<T>>(newBufferState);
        }
    }
}