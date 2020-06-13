using Light.GuardClauses;

namespace Light.Json.Buffers
{
    public abstract class BaseBufferProvider
    {
        protected BaseBufferProvider(int maximumArraySizeInByte,
                                     IIncreaseBufferSizeStrategy? increaseBufferSizeStrategy = null,
                                     int initialBufferSizeInByte = 10240)
        {
            IncreaseBufferSizeStrategy = increaseBufferSizeStrategy ?? new DoubleArraySizeStrategy();
            InitialBufferSizeInByte = initialBufferSizeInByte.MustBeGreaterThan(1, nameof(initialBufferSizeInByte));
            MaximumBufferSizeInByte = maximumArraySizeInByte.MustBeGreaterThanOrEqualTo(initialBufferSizeInByte, nameof(maximumArraySizeInByte));
        }

        public IIncreaseBufferSizeStrategy IncreaseBufferSizeStrategy { get; }
        public int InitialBufferSizeInByte { get; }
        public int MaximumBufferSizeInByte { get; }
    }
}