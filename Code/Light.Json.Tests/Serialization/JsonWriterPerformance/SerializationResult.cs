using System;
using Light.Json.Buffers;

namespace Light.Json.Tests.Serialization.JsonWriterPerformance
{
    public readonly struct SerializationResult<T> : IDisposable
    {
        private readonly T[] _buffer;
        private readonly IBufferProvider<T> _bufferProvider;

        public SerializationResult(Memory<T> json, T[] buffer, IBufferProvider<T> bufferProvider)
        {
            Json = json;
            _buffer = buffer;
            _bufferProvider = bufferProvider;
        }

        public Memory<T> Json { get; }

        public void Dispose() => _bufferProvider.Finish(_buffer);
    }
}