using System.Buffers;
using Light.GuardClauses;

namespace Light.Json.Streaming
{
    public sealed class ArrayPoolTextBufferProvider : ITextBufferProvider
    {
        private readonly ArrayPool<char> _arrayPool;
        private readonly int _bufferSize;
        private readonly bool _isBufferClearedOnReturn;

        public ArrayPoolTextBufferProvider(ArrayPool<char> arrayPool, int bufferSize = 1024, bool isBufferClearedOnReturn = false)
        {
            _arrayPool = arrayPool ?? ArrayPool<char>.Shared;
            _bufferSize = bufferSize.MustNotBeLessThan(2, nameof(bufferSize));
            _isBufferClearedOnReturn = isBufferClearedOnReturn;
        }

        public char[] RentBuffer() =>
            _arrayPool.Rent(_bufferSize);

        public void ReturnBuffer(char[] buffer) =>
            _arrayPool.Return(buffer, _isBufferClearedOnReturn);
    }
}