using System.Buffers;
using System.IO;
using System.Threading.Tasks;
using Light.GuardClauses;

namespace Light.Json.Buffers
{
    public sealed class StreamingArrayPoolBufferWriterService : BaseArrayPoolBufferWriterService<byte>
    {
        public StreamingArrayPoolBufferWriterService(Stream stream,
                                                     ArrayPool<byte>? arrayPool = null,
                                                     int initialArraySizeInByte = 262_144,
                                                     int? maximumArraySizeInByte = null,
                                                     IIncreaseBufferSizeStrategy? increaseBufferSizeStrategy = null)
            : base(arrayPool, initialArraySizeInByte, maximumArraySizeInByte, increaseBufferSizeStrategy)
        {
            Stream = stream.MustNotBeNull(nameof(stream));
        }

        public Stream Stream { get; }

        public override async ValueTask<BufferState<byte>> ProcessFullBufferAsync(BufferState<byte> bufferState, int numberOfAdditionalSlots)
        {
            if (bufferState.CurrentIndex > 0)
                await Stream.WriteAsync(bufferState.CurrentBuffer, 0, bufferState.CurrentIndex);

            if (numberOfAdditionalSlots < bufferState.CurrentBuffer.Length)
                return new BufferState<byte>(bufferState.CurrentBuffer, 0, numberOfAdditionalSlots);

            var newSize = IncreaseBufferSizeStrategy.DetermineNewBufferSize(bufferState.CurrentBuffer.Length, numberOfAdditionalSlots, MaximumBufferSizeInByte);
            var newBuffer = ArrayPool.Rent(newSize);
            ArrayPool.Return(bufferState.CurrentBuffer);
            return new BufferState<byte>(newBuffer, 0, numberOfAdditionalSlots);
        }
    }
}