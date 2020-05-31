using System.Buffers;
using System.Threading.Tasks;
using Light.GuardClauses;

namespace Light.Json.Buffers
{
    public abstract class BaseArrayPoolBufferWriterService<T> : IBufferWriterService<T>
    {
        protected BaseArrayPoolBufferWriterService(ArrayPool<T>? arrayPool = null,
                                                   int initialArraySizeInByte = 262_144,
                                                   int? maximumArraySizeInByte = null,
                                                   IIncreaseBufferSizeStrategy? increaseBufferSizeStrategy = null)
        {
            MaximumBufferSizeInByte = maximumArraySizeInByte?.MustBeGreaterThanOrEqualTo(initialArraySizeInByte) ?? MaximumArraySize.GetMaximumArraySizeInByte<T>();
            ArrayPool = arrayPool ?? ArrayPool<T>.Shared;
            InitialBufferSizeInByte = initialArraySizeInByte.MustBeIn(Range.FromInclusive(1).ToInclusive(MaximumBufferSizeInByte));
            IncreaseBufferSizeStrategy = increaseBufferSizeStrategy ?? new MultiplyBufferSizeStrategy();
        }

        public ArrayPool<T> ArrayPool { get; }

        public int MaximumBufferSizeInByte { get; }

        public int InitialBufferSizeInByte { get; }

        public IIncreaseBufferSizeStrategy IncreaseBufferSizeStrategy { get; }

        public T[] GetInitialBuffer() => ArrayPool.Rent(InitialBufferSizeInByte);

        public abstract ValueTask<BufferState<T>> ProcessFullBufferAsync(BufferState<T> bufferState, int numberOfAdditionalSlots);

        public void ReturnBuffer(T[] currentBuffer) => ArrayPool.Return(currentBuffer);
    }
}