namespace Light.Json.Serialization.LowLevelWriting
{
    public interface IInMemoryBufferProvider<T>
    {
        T[] GetInitialBuffer();

        T[] GetNewBufferWithIncreasedSize(T[] currentBuffer, int numberOfRequiredBufferSlots);

        void Finish(T[] currentBuffer);
    }
}