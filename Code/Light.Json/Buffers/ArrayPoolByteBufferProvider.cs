using System;
using System.Buffers;

namespace Light.Json.Buffers
{
    public sealed class ArrayPoolByteBufferProvider : BaseArrayPoolBufferProvider<byte>, IBufferProvider<byte>
    {
        public ArrayPoolByteBufferProvider(ArrayPool<byte>? arrayPool = null,
                                           int initialBufferSizeInByte = 262_144,
                                           int? maximumBufferSizeInByte = null,
                                           IIncreaseBufferSizeStrategy? increaseBufferSizeStrategy = null)
            : base(maximumBufferSizeInByte ?? MaximumArraySize.GetMaximumByteArraySize(), arrayPool, initialBufferSizeInByte, increaseBufferSizeStrategy) { }

        public unsafe byte[] GetNewBufferWithIncreasedSize(byte[] currentBuffer, int numberOfAdditionalSlots)
        {
            var newSize = IncreaseBufferSizeStrategy.DetermineNewBufferSize(currentBuffer.Length, numberOfAdditionalSlots, MaximumBufferSizeInByte);
            var newBuffer = ArrayPool.Rent(newSize);
            fixed (void* source = &currentBuffer[0], target = &newBuffer[0])
                Buffer.MemoryCopy(source, target, newSize, currentBuffer.Length);
            currentBuffer.CopyTo(newBuffer, 0);
            ArrayPool.Return(currentBuffer);
            return newBuffer;
        }
    }
}