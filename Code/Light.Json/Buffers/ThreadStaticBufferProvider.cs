using System;
using Light.GuardClauses;

namespace Light.Json.Buffers
{
    public sealed class ThreadStaticByteBufferProvider : IBufferProvider<byte>
    {
        [ThreadStatic] private static byte[]? _buffer;

        public ThreadStaticByteBufferProvider(IIncreaseBufferSizeStrategy? increaseBufferSizeStrategy = null,
                                              int initialBufferSizeInByte = 84975,
                                              int? maximumArraySizeInByte = null)
        {
            IncreaseBufferSizeStrategy = increaseBufferSizeStrategy ?? new MultiplyBufferSizeStrategy();
            MaximumArraySizeInByte = maximumArraySizeInByte ?? MaximumArraySize.GetMaximumArraySizeInByte<byte>();
            if (_buffer == null || _buffer.Length < initialBufferSizeInByte.MustBeGreaterThan(0, nameof(initialBufferSizeInByte)))
                _buffer = new byte[initialBufferSizeInByte];
        }


        public IIncreaseBufferSizeStrategy IncreaseBufferSizeStrategy { get; }
        public int MaximumArraySizeInByte { get; }

        public byte[] GetInitialBuffer() => _buffer!;

        public unsafe byte[] GetNewBufferWithIncreasedSize(byte[] currentBuffer, int numberOfAdditionalSlots)
        {
            numberOfAdditionalSlots.MustBeGreaterThan(0, nameof(numberOfAdditionalSlots));

            var newSize = IncreaseBufferSizeStrategy.DetermineNewBufferSize(currentBuffer.Length, numberOfAdditionalSlots, MaximumArraySizeInByte);
            var newBuffer = new byte[newSize];
            fixed (void* source = &currentBuffer[0], target = &newBuffer[0])
                Buffer.MemoryCopy(source, target, newSize, currentBuffer.Length);
            _buffer = newBuffer;
            return newBuffer;
        }

        public void Finish(byte[] currentBuffer) { }
    }
}