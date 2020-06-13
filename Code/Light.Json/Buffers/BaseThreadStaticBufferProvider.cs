using System;

namespace Light.Json.Buffers
{
    public abstract class BaseThreadStaticBufferProvider<T> : BaseBufferProvider
    {
        [ThreadStatic]
        protected static T[]? CurrentBufferOfThread;

        protected BaseThreadStaticBufferProvider(int maximumArraySizeInByte,
                                                 IIncreaseBufferSizeStrategy? increaseBufferSizeStrategy = null,
                                                 int initialBufferSizeInByte = 10240)
            : base(maximumArraySizeInByte, increaseBufferSizeStrategy, initialBufferSizeInByte) { }


        public T[] GetInitialBuffer() => CurrentBufferOfThread ??= new T[InitialBufferSizeInByte];

        public void ReturnBuffer(T[] currentBuffer) { }
    }
}