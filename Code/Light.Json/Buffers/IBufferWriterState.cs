namespace Light.Json.Buffers
{
    public interface IBufferWriterState<T>
    {
        IBufferWriterService<T> BufferWriterService { get; }

        T[] CurrentBuffer { get; set; }

        int CurrentIndex { get; set; }

        int EnsuredIndex { get; set; }
    }
}