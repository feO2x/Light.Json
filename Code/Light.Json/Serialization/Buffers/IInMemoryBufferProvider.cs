namespace Light.Json.Serialization.Buffers
{
    public interface IInMemoryBufferProvider<T>
    {
        T[] GetInitialBuffer();

        T[] GetNewBufferWithIncreasedSize(T[] currentBuffer);

        void Finish(T[] currentBuffer);
    }
}