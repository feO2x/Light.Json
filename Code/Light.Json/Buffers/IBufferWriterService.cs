using System.Threading.Tasks;

namespace Light.Json.Buffers
{
    public interface IBufferWriterService<T>
    {
        T[] GetInitialBuffer();

        ValueTask<BufferState<T>> ProcessFullBufferAsync(BufferState<T> bufferState, int numberOfAdditionalSlots);

        void ReturnBuffer(T[] currentBuffer);
    }
}