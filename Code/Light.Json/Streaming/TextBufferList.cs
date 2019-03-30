using System;
using System.Runtime.CompilerServices;
using Light.GuardClauses;

namespace Light.Json.Streaming
{
    public struct TextBufferList
    {
        private TextBuffer[] _buffers;
        private int _count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TextBufferList(int initialSize)
        {
            initialSize.MustNotBeLessThan(1, nameof(initialSize));
            _buffers = new TextBuffer[initialSize];
            _count = 0;
        }

        public int Count => _count;

        public ref TextBuffer LatestBuffer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_count == 0)
                    ThrowInvalidAccess();
                return ref _buffers[_count - 1];
            }
        }

        private static void ThrowInvalidAccess()
        {
            throw new InvalidOperationException("You cannot access the latest buffer when count is null.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddBuffer(in TextBuffer buffer)
        {
            buffer.MustNotBeDefault(nameof(buffer));
            if (_count == _buffers.Length)
                IncreaseBuffersArray();

            _buffers[_count++] = buffer;
        }

        private void IncreaseBuffersArray()
        {
            var newArray = new TextBuffer[_count * 2];
            _buffers.CopyTo(newArray.AsMemory());
            _buffers = newArray;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<TextBuffer> GetOldBuffers() =>
            _count > 1 ? new ReadOnlySpan<TextBuffer>(_buffers, 0, _count - 1) : default;

        public void ReleaseOldBuffers()
        {
            if (_count <= 1)
                return;

            _buffers[0] = _buffers[_count - 1];
            for (var i = 1; i < _buffers.Length; ++i)
            {
                _buffers[i] = default;
            }

            _count = 1;
        }
    }
}