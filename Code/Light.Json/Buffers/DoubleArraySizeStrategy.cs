using System.Runtime.Serialization;
using Light.GuardClauses;

namespace Light.Json.Buffers
{
    public sealed class DoubleArraySizeStrategy : IIncreaseBufferSizeStrategy
    {
        public int DetermineNewBufferSize(int currentSize, int numberOfAdditionallyRequiredSlots, int maximumArraySizeInByte)
        {
            currentSize.MustBeGreaterThan(0, nameof(currentSize));
            numberOfAdditionallyRequiredSlots.MustBeGreaterThan(0, nameof(numberOfAdditionallyRequiredSlots));
            maximumArraySizeInByte.MustBeGreaterThan(0, nameof(maximumArraySizeInByte));

            var newMinimumSizeInByte = currentSize + numberOfAdditionallyRequiredSlots;
            if (maximumArraySizeInByte < newMinimumSizeInByte)
                throw new SerializationException($"Maximum buffer size exceeded: the required new size of {newMinimumSizeInByte:N0} bytes is larger than the maximum array size of {maximumArraySizeInByte:N0} bytes.");

            var newSize = currentSize + currentSize;
            if (newSize < newMinimumSizeInByte)
                newSize = newMinimumSizeInByte;
            if (newSize > maximumArraySizeInByte)
                newSize = maximumArraySizeInByte;
            return newSize;
        }
    }
}