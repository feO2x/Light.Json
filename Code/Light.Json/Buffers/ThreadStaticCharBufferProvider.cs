using System;
using Light.GuardClauses;

namespace Light.Json.Buffers
{
    public sealed class ThreadStaticCharBufferProvider : BaseThreadStaticBufferProvider<char>, IBufferProvider<char>
    {
        public ThreadStaticCharBufferProvider(IIncreaseBufferSizeStrategy? increaseBufferSizeStrategy = null,
                                              int initialBufferSizeInByte = 10240,
                                              int? maximumArraySizeInByte = null)
            : base(maximumArraySizeInByte ?? MaximumArraySize.GetMaximumCharArraySize(), increaseBufferSizeStrategy, initialBufferSizeInByte) { }

        public unsafe char[] GetNewBufferWithIncreasedSize(char[] currentBuffer, int numberOfAdditionalSlots)
        {
            numberOfAdditionalSlots.MustBeGreaterThan(0, nameof(numberOfAdditionalSlots));

            var newSize = IncreaseBufferSizeStrategy.DetermineNewBufferSize(currentBuffer.Length, numberOfAdditionalSlots, MaximumBufferSizeInByte);
            var newBuffer = new char[newSize];
            fixed (void* source = &currentBuffer[0], target = &newBuffer[0])
                Buffer.MemoryCopy(source, target, newSize + newSize, currentBuffer.Length + currentBuffer.Length); // As char has double the size of byte, we need to copy double the amount of bytes
            CurrentBufferOfThread = newBuffer;
            return newBuffer;
        }
    }
}