using System.Threading.Tasks;
using Light.Json.Buffers;

namespace Light.Json.Serialization.LowLevelWriting
{
    public sealed class AsyncJsonWriter<TBufferWriter> : IAsyncJsonWriter<TBufferWriter>
        where TBufferWriter : struct, IBufferWriter
    {
        private TBufferWriter _bufferWriter;

        public AsyncJsonWriter(TBufferWriter bufferWriter)
        {
            _bufferWriter = bufferWriter;
        }

        public ref TBufferWriter BufferWriter => ref _bufferWriter;

        public Task WriteBeginOfObjectAsync() => WriteSingleAsciiCharacterAsync('{');

        public Task WriteEndOfObjectAsync() => WriteSingleAsciiCharacterAsync('}');

        public Task WriteBeginOfArrayAsync() => WriteSingleAsciiCharacterAsync('[');

        public Task WriteEndOfArrayAsync() => WriteSingleAsciiCharacterAsync(']');

        public Task WriteKeyValueSeparatorAsync() => WriteSingleAsciiCharacterAsync(':');

        public Task WriteValueSeparatorAsync() => WriteSingleAsciiCharacterAsync(',');

        private Task WriteSingleAsciiCharacterAsync(char character)
        {
            var capacityTask = _bufferWriter.EnsureCapacityFromCurrentIndexAsync(1);
            if (capacityTask.Status != TaskStatus.RanToCompletion)
                return AwaitCapacityForSingleAsciiCharacterAsync(capacityTask, character);

            _bufferWriter.WriteAscii(character);
            return Task.CompletedTask;
        }

        private async Task AwaitCapacityForSingleAsciiCharacterAsync(Task capacityTask, char character)
        {
            await capacityTask.ConfigureAwait(false);
            _bufferWriter.WriteAscii(character);
        }
    }
}