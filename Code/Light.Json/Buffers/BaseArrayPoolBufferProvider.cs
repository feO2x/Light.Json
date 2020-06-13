using System.Buffers;

namespace Light.Json.Buffers
{
    public abstract class BaseArrayPoolBufferProvider<T> : BaseBufferProvider
    {
        protected BaseArrayPoolBufferProvider(int maximumBufferSizeInByte,
                                              ArrayPool<T>? arrayPool = null,
                                              int initialBufferSizeInByte = 262_144,
                                              IIncreaseBufferSizeStrategy? increaseBufferSizeStrategy = null)
            : base(maximumBufferSizeInByte, increaseBufferSizeStrategy, initialBufferSizeInByte)
        {
            ArrayPool = arrayPool ?? ArrayPool<T>.Shared;
        }

        public ArrayPool<T> ArrayPool { get; }

        public T[] GetInitialBuffer() => ArrayPool.Rent(InitialBufferSizeInByte);

        public void ReturnBuffer(T[] currentBuffer) => ArrayPool.Return(currentBuffer);
    }
}