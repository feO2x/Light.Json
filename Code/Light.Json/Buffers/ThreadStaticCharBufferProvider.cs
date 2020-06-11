using System;
using Light.GuardClauses;

namespace Light.Json.Buffers
{
    public sealed class ThreadStaticCharBufferProvider : IBufferProvider<char>
    {
        [ThreadStatic] private static char[]? _buffer;

        public ThreadStaticCharBufferProvider(IIncreaseBufferSizeStrategy? increaseBufferSizeStrategy = null,
                                              int initialBufferSizeInByte = 10240,
                                              int? maximumArraySizeInByte = null)
        {
            IncreaseBufferSizeStrategy = increaseBufferSizeStrategy ?? new MultiplyBufferSizeStrategy();
            MaximumArraySizeInByte = maximumArraySizeInByte ?? MaximumArraySize.GetMaximumArraySizeInByte<char>();
            if (_buffer == null || _buffer.Length < initialBufferSizeInByte.MustBeGreaterThan(0, nameof(initialBufferSizeInByte)))
                _buffer = new char[initialBufferSizeInByte];
        }

        public IIncreaseBufferSizeStrategy IncreaseBufferSizeStrategy { get; }
        public int MaximumArraySizeInByte { get; }

        public char[] GetInitialBuffer() => _buffer!;

        public unsafe char[] GetNewBufferWithIncreasedSize(char[] currentBuffer, int numberOfAdditionalSlots)
        {
            numberOfAdditionalSlots.MustBeGreaterThan(0, nameof(numberOfAdditionalSlots));

            var newSize = IncreaseBufferSizeStrategy.DetermineNewBufferSize(currentBuffer.Length, numberOfAdditionalSlots, MaximumArraySizeInByte);
            var newBuffer = new char[newSize];
            fixed (void* source = &currentBuffer[0], target = &newBuffer[0])
                Buffer.MemoryCopy(source, target, newSize, currentBuffer.Length);
            _buffer = newBuffer;
            return newBuffer;
        }

        public void ReturnBuffer(char[] currentBuffer) { }
    }
}