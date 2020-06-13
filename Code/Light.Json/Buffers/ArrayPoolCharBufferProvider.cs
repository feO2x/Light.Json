using System;
using System.Buffers;

namespace Light.Json.Buffers
{
    public sealed class ArrayPoolCharBufferProvider : BaseArrayPoolBufferProvider<char>, IBufferProvider<char>
    {
        public ArrayPoolCharBufferProvider(ArrayPool<char>? arrayPool = null,
                                           int initialBufferSizeInByte = 262_144,
                                           int? maximumBufferSizeInByte = null,
                                           IIncreaseBufferSizeStrategy? increaseBufferSizeStrategy = null)
            : base(maximumBufferSizeInByte ?? MaximumArraySize.GetMaximumByteArraySize(), arrayPool, initialBufferSizeInByte, increaseBufferSizeStrategy) { }

        public unsafe char[] GetNewBufferWithIncreasedSize(char[] currentBuffer, int numberOfAdditionalSlots)
        {
            var newSize = IncreaseBufferSizeStrategy.DetermineNewBufferSize(currentBuffer.Length, numberOfAdditionalSlots, MaximumBufferSizeInByte);
            var newBuffer = ArrayPool.Rent(newSize);
            fixed (void* source = &currentBuffer[0], target = &newBuffer[0])
                Buffer.MemoryCopy(source, target, newSize + newSize, currentBuffer.Length + currentBuffer.Length);
            currentBuffer.CopyTo(newBuffer, 0);
            ArrayPool.Return(currentBuffer);
            return newBuffer;
        }
    }
}