using System;
using System.Runtime.CompilerServices;
using Light.GuardClauses;
using Light.GuardClauses.Exceptions;

namespace Light.Json.Streaming
{
    public struct TextBuffer
    {
        private readonly char[] _buffer;
        private int _currentIndex;

        public TextBuffer(char[] buffer)
        {
            _buffer = buffer.MustNotBeNull(nameof(buffer));
            _currentIndex = 0;
        }

        public bool IsDefault
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer == null;
        }


        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MustNotBeDefault(string parameterName)
        {
            if (IsDefault)
                ThrowMustNotBeDefault(parameterName);
        }

        private static void ThrowMustNotBeDefault(string parameterName) =>
            throw new ArgumentDefaultException($"{parameterName ?? "The text buffer"} must not be the default instance.");

        public bool IsAtEnd
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _currentIndex == _buffer.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetNextCharacter(out char character)
        {
            if (IsAtEnd)
            {
                character = default;
                return false;
            }

            character = _buffer[_currentIndex++];
            return true;
        }

        public TextBufferPosition CurrentPosition
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new TextBufferPosition(_buffer, _currentIndex);
        }

        public ReadOnlySpan<char> Span
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _buffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<char> Slice(int startIndex, int endIndex) =>
            new ReadOnlySpan<char>(_buffer, startIndex, endIndex - startIndex + 1);

        public ReadOnlySpan<char> Slice(int startIndex) =>
            new ReadOnlySpan<char>(_buffer, startIndex, _buffer.Length - startIndex);
    }
}