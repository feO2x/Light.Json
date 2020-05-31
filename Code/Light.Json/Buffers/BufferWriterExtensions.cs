namespace Light.Json.Buffers
{
    public static class BufferWriterExtensions
    {
        public static BufferState<T> GetBufferState<TBufferWriter, T>(this ref TBufferWriter source)
            where TBufferWriter : struct, IBufferWriterState<T>
        {
            return new BufferState<T>(source.CurrentBuffer, source.CurrentIndex, source.EnsuredIndex);
        }

        public static void ApplyBufferState<TBufferWriter, T>(this ref TBufferWriter target, in BufferState<T> bufferState)
            where TBufferWriter : struct, IBufferWriterState<T>
        {
            target.CurrentBuffer = bufferState.CurrentBuffer;
            target.CurrentIndex = bufferState.CurrentIndex;
            target.EnsuredIndex = bufferState.EnsuredIndex;
        }
    }
}