using System.Threading.Tasks;
using Light.Json.Buffers;

namespace Light.Json.Serialization.LowLevelWriting
{
    public interface IAsyncJsonWriter<TBufferWriter>
        where TBufferWriter : struct, IBufferWriter
    {
        ref TBufferWriter BufferWriter { get; }

        Task WriteBeginOfObjectAsync();

        Task WriteEndOfObjectAsync();

        Task WriteBeginOfArrayAsync();

        Task WriteEndOfArrayAsync();

        Task WriteKeyValueSeparatorAsync();

        Task WriteValueSeparatorAsync();
    }
}