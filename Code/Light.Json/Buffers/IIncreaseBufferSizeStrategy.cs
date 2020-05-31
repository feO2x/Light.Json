namespace Light.Json.Buffers
{
    public interface IIncreaseBufferSizeStrategy
    {
        int DetermineNewBufferSize(int currentSize, int numberOfAdditionallyRequiredSlots, int maximumArraySizeInByte);
    }
}