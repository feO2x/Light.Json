using System;
using Light.Json.Buffers;

namespace Light.Json
{
    public readonly struct Utf8SerializationResult : IDisposable
    {
        private readonly byte[] _currentBuffer;
        private readonly IBufferProvider<byte> _currentBufferProvider;

        public Utf8SerializationResult(Memory<byte> json, byte[] currentBuffer, IBufferProvider<byte> currentBufferProvider)
        {
            Json = json;
            _currentBuffer = currentBuffer;
            _currentBufferProvider = currentBufferProvider;
        }

        public Memory<byte> Json { get; }

        public void Dispose() => _currentBufferProvider.ReturnBuffer(_currentBuffer);

        public byte[] GetJsonAsByteArrayAndDisposeResult()
        {
            var array = Json.ToArray();
            Dispose();
            return array;
        }
    }
}