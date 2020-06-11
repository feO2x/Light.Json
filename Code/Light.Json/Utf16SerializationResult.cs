using System;
using Light.GuardClauses;
using Light.Json.Buffers;

namespace Light.Json
{
    public readonly struct Utf16SerializationResult : IDisposable
    {
        private readonly char[] _currentBuffer;
        private readonly IBufferProvider<char> _currentBufferProvider;

        public Utf16SerializationResult(Memory<char> json, char[] currentBuffer, IBufferProvider<char> currentBufferProvider)
        {
            Json = json;
            _currentBuffer = currentBuffer.MustNotBeNull(nameof(currentBuffer));
            _currentBufferProvider = currentBufferProvider.MustNotBeNull(nameof(currentBufferProvider));
        }

        public Memory<char> Json { get; }

        public string GetJsonAsStringAndDisposeResult()
        {
            var jsonString = Json.ToString();
            Dispose();
            return jsonString;
        }

        public void Dispose() => _currentBufferProvider.ReturnBuffer(_currentBuffer);

        public override string ToString() => Json.ToString();
    }
}