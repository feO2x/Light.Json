using System.Buffers;

namespace Light.Json
{
    public ref struct ReadOnlySequenceItemEnumerator<T>
    {
        public readonly ReadOnlySequence<T> _sequence;
        private int _currentBufferIndex;
        private ReadOnlySequence<T>.Enumerator _enumerator;

        public ReadOnlySequenceItemEnumerator(ReadOnlySequence<T> sequence)
        {
            _sequence = sequence;
            _enumerator = sequence.GetEnumerator();
            _currentBufferIndex = _enumerator.MoveNext() ? 0 : -2;
        }

        public bool TryGetNext(out T item)
        {
            if (_currentBufferIndex == -2)
            {
                item = default;
                return false;
            }

            var span = _enumerator.Current.Span;
            if (_currentBufferIndex >= span.Length)
            {
                if (!TryMoveToNextBuffer())
                {
                    item = default;
                    return false;
                }
            }

            item = span[_currentBufferIndex++];
            return true;
        }

        private bool TryMoveToNextBuffer()
        {
            if (!_enumerator.MoveNext())
            {
                _currentBufferIndex = -2;
                return false;
            }

            _currentBufferIndex = 0;
            return true;
        }
    }
}