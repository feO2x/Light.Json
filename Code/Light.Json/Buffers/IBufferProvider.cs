namespace Light.Json.Buffers
{
    public interface IBufferProvider<T>
    {
        T[] GetInitialBuffer();

        T[] GetNewBufferWithIncreasedSize(T[] currentBuffer, int numberOfAdditionalSlots);

        void Finish(T[] currentBuffer);
    }
}