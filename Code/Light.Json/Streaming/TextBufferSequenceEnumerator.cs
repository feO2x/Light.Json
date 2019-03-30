using System;

namespace Light.Json.Streaming
{
    public ref struct TextBufferSequenceEnumerator
    {
        private readonly TextBufferSequence _sequence;
        private int _bufferIndex;
        private ReadOnlySpan<char> _currentSpan;
        private int _currentSpanIndex;

        public TextBufferSequenceEnumerator(in TextBufferSequence sequence)
        {
            _sequence = sequence;
            _currentSpanIndex = default;
            if (sequence.IsEmpty)
            {
                _bufferIndex = -2;
                _currentSpan = default;
                return;
            }

            if (sequence.TryGetSingleSpan(out _currentSpan))
            {
                _bufferIndex = -3;
                return;
            }

            _currentSpan = sequence.Buffers[0].Slice(sequence.StartIndexOfFirstBuffer);
            _bufferIndex = 0;
        }

        public bool TryGetNext(out char character)
        {
            if (_bufferIndex == -2)
            {
                character = default;
                return false;
            }

            if (_currentSpanIndex == _currentSpan.Length)
            {
                if (!TryMoveToNextBuffer())
                {
                    character = default;
                    return false;
                }
            }

            character = _currentSpan[_currentSpanIndex++];
            return true;
        }

        private bool TryMoveToNextBuffer()
        {
            if (_bufferIndex == -3 ||
                _bufferIndex + 1 == _sequence.BufferCount)
            {
                _bufferIndex = -2;
                return false;
            }

            ++_bufferIndex;

            _currentSpan = _bufferIndex == _sequence.BufferCount - 1 ? _sequence[_bufferIndex].Slice(0, _sequence.EndIndexOfLastBuffer) : _sequence[_bufferIndex].Span;
            _currentSpanIndex = 0;
            return true;
        }
    }
}