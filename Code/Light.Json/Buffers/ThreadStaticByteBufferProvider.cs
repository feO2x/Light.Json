using System;
using Light.GuardClauses;

namespace Light.Json.Buffers
{
    public sealed class ThreadStaticByteBufferProvider : BaseThreadStaticBufferProvider<byte>, IBufferProvider<byte>
    {
        public ThreadStaticByteBufferProvider(IIncreaseBufferSizeStrategy? increaseBufferSizeStrategy = null,
                                              int initialBufferSizeInByte = 10240,
                                              int? maximumArraySizeInByte = null)
            : base(maximumArraySizeInByte ?? MaximumArraySize.GetMaximumByteArraySize(), increaseBufferSizeStrategy, initialBufferSizeInByte) { }

        public unsafe byte[] GetNewBufferWithIncreasedSize(byte[] currentBuffer, int numberOfAdditionalSlots)
        {
            numberOfAdditionalSlots.MustBeGreaterThan(0, nameof(numberOfAdditionalSlots));

            var newSize = IncreaseBufferSizeStrategy.DetermineNewBufferSize(currentBuffer.Length, numberOfAdditionalSlots, MaximumBufferSizeInByte);
            var newBuffer = new byte[newSize];
            fixed (void* source = &currentBuffer[0], target = &newBuffer[0])
                Buffer.MemoryCopy(source, target, newSize, currentBuffer.Length);
            CurrentBufferOfThread = newBuffer;
            return newBuffer;
        }
    }
}