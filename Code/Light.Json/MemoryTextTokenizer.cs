using System;
using Light.GuardClauses;

namespace Light.Json
{
    public struct MemoryTextTokenizer : ITextTokenizer
    {
        private static readonly string NewLineCharacters = Environment.NewLine;
        private static readonly char NewLineFirstCharacter = NewLineCharacters[0];
        private readonly ReadOnlyMemory<char> _json;
        private int _currentIndex;
        private int _currentLine;
        private int _currentPosition;

        public MemoryTextTokenizer(ReadOnlyMemory<char> json)
        {
            _json = json;
            _currentIndex = 0;
            _currentLine = 1;
            _currentPosition = 1;
        }

        public int CurrentIndex => _currentIndex;

        public int CurrentLine => _currentLine;

        public JsonTextToken GetNextToken()
        {
            var span = _json.Span;
            if (!TryReadNextCharacter(span, out var currentCharacter))
                return new JsonTextToken(JsonTokenType.EndOfDocument);
            switch (currentCharacter)
            {
                case JsonTokenizerSymbols.StringDelimiter:
                    return ReadString(span);
                case JsonTokenizerSymbols.FalseStartCharacter:
                    return ReadConstant(span, JsonTokenType.False, JsonTokenizerSymbols.False);
                case JsonTokenizerSymbols.TrueStartCharacter:
                    return ReadConstant(span, JsonTokenType.True, JsonTokenizerSymbols.True);
                case JsonTokenizerSymbols.NullStartCharacter:
                    return ReadConstant(span, JsonTokenType.Null, JsonTokenizerSymbols.Null);
                case JsonTokenizerSymbols.MinusSign:
                    return ReadNegativeNumber(span);
                case JsonTokenizerSymbols.BeginOfObject:
                    return ReadSingleCharacter(span, JsonTokenType.BeginOfObject);
                case JsonTokenizerSymbols.EndOfObject:
                    return ReadSingleCharacter(span, JsonTokenType.EndOfObject);
                case JsonTokenizerSymbols.BeginOfArray:
                    return ReadSingleCharacter(span, JsonTokenType.BeginOfArray);
                case JsonTokenizerSymbols.EndOfArray:
                    return ReadSingleCharacter(span, JsonTokenType.EndOfArray);
                case JsonTokenizerSymbols.EntrySeparator:
                    return ReadSingleCharacter(span, JsonTokenType.EntrySeparator);
                case JsonTokenizerSymbols.NameValueSeparator:
                    return ReadSingleCharacter(span, JsonTokenType.NameValueSeparator);
            }

            if (char.IsDigit(currentCharacter))
                return ReadNumber(span);

            throw new DeserializationException($"Unexpected character \"{currentCharacter}\" at line {_currentLine} position {_currentPosition}.");
        }

        private bool TryReadNextCharacter(in ReadOnlySpan<char> json, out char currentCharacter)
        {
            // In this method, we search for the first character of the next token.
            var isInSingleLineComment = false;
            while (_currentIndex < _json.Length)
            {
                currentCharacter = json[_currentIndex];

                // If the current character is not white space and we are not in a single line comment, then return it
                if (!currentCharacter.IsWhiteSpace() && !isInSingleLineComment)
                {
                    // Check if the character is the beginning of a single line comment.
                    // If not, it can be returned and processed.
                    if (currentCharacter != JsonTokenizerSymbols.SingleLineCommentCharacter)
                        return true;

                    // If it is, then check if there is enough space for another slash.
                    // If not, then return the current character, as this will result
                    // in an exception reporting an unexpected character.
                    if (_currentIndex + 1 >= _json.Length)
                        return true;

                    // Else check if the next character is actually the second slash of a comment.
                    // If it is not, then return the slash as this will result in an exception
                    // reporting an unexpected character (as above).
                    currentCharacter = json[_currentIndex + 1];
                    if (currentCharacter != JsonTokenizerSymbols.SingleLineCommentCharacter)
                    {
                        currentCharacter = '/';
                        return true;
                    }

                    // Else we are in a single line comment until we find a new line
                    isInSingleLineComment = true;
                    _currentIndex += 2;
                    _currentPosition += 2;
                    continue;
                }

                // If the current Character is not a new line charter, advance position and index
                if (currentCharacter != NewLineFirstCharacter)
                {
                    ++_currentIndex;
                    ++_currentPosition;
                    continue;
                }

                // Handle new line characters with length 1
                if (NewLineCharacters.Length == 1)
                {
                    ++_currentIndex;
                    ++_currentLine;
                    _currentPosition = 1;
                    isInSingleLineComment = false;
                    continue;
                }

                // Check if there is enough space for a second line character
                if (_currentIndex + 2 >= _json.Length)
                {
                    currentCharacter = default;
                    return false;
                }

                currentCharacter = json[++_currentIndex];
                ++_currentPosition;
                if (currentCharacter == NewLineCharacters[1])
                {
                    ++_currentIndex;
                    ++_currentLine;
                    _currentPosition = 1;
                    isInSingleLineComment = false;
                }
            }

            currentCharacter = default;
            return false;
        }

        private JsonTextToken ReadSingleCharacter(in ReadOnlySpan<char> json, JsonTokenType tokenType) =>
            new JsonTextToken(tokenType, json.Slice(_currentIndex++, 1));

        private JsonTextToken ReadNumber(in ReadOnlySpan<char> json)
        {
            int i;
            for (i = _currentIndex + 1; i < _json.Length; ++i)
            {
                var currentCharacter = json[i];
                if (char.IsDigit(currentCharacter))
                    continue;

                if (currentCharacter == JsonTokenizerSymbols.DecimalSymbol)
                    return ReadFloatingPointNumber(json, i);
                break;
            }

            var token = new JsonTextToken(JsonTokenType.IntegerNumber, json.Slice(_currentIndex, i - _currentIndex));
            _currentIndex = i;
            _currentPosition += token.Text.Length;
            return token;
        }

        private JsonTextToken ReadFloatingPointNumber(in ReadOnlySpan<char> json, int decimalSymbolIndex)
        {
            int i;
            for (i = decimalSymbolIndex + 1; i < _json.Length; i++)
            {
                if (!char.IsDigit(json[i]))
                    break;
            }

            var textSpan = json.Slice(_currentIndex, i - _currentIndex);
            if (i == decimalSymbolIndex + 1)
                Throw($"Expected digit after decimal symbol in token \"{textSpan.ToString()}\" at line {_currentLine} position {_currentPosition}.");

            var token = new JsonTextToken(JsonTokenType.FloatingPointNumber, textSpan);
            _currentIndex = i;
            _currentPosition += token.Text.Length;
            return token;
        }

        private JsonTextToken ReadNegativeNumber(in ReadOnlySpan<char> json)
        {
            int i;
            for (i = _currentIndex + 1; i < _json.Length; ++i)
            {
                var currentCharacter = json[i];
                if (char.IsDigit(currentCharacter))
                    continue;

                if (currentCharacter == JsonTokenizerSymbols.DecimalSymbol)
                    return ReadFloatingPointNumber(json, i);
                break;
            }

            if (i == _currentIndex + 1)
                Throw($"Expected digit after minus sign at line {_currentLine} position {_currentPosition}.");

            var token = new JsonTextToken(JsonTokenType.IntegerNumber, json.Slice(_currentIndex, i - _currentIndex));
            _currentIndex = i;
            _currentPosition += token.Text.Length;
            return token;
        }

        private JsonTextToken ReadConstant(in ReadOnlySpan<char> json, JsonTokenType type, string expectedTokenText)
        {
            if (_currentIndex + expectedTokenText.Length > json.Length)
                ThrowInvalidConstant(expectedTokenText, json.Slice(_currentIndex));
            var constantTokenText = json.Slice(_currentIndex, expectedTokenText.Length);
            if (!constantTokenText.Equals(expectedTokenText.AsSpan(), StringComparison.Ordinal))
                ThrowInvalidConstant(expectedTokenText, constantTokenText);
            _currentIndex += expectedTokenText.Length;
            return new JsonTextToken(type, constantTokenText);
        }

        private JsonTextToken ReadString(in ReadOnlySpan<char> json)
        {
            var leftBoundedJson = json.Slice(_currentIndex);

            for (var i = 1; i < leftBoundedJson.Length; ++i)
            {
                if (leftBoundedJson[i] != JsonTokenizerSymbols.StringDelimiter)
                    continue;

                var previousIndex = i - 1;
                if (previousIndex > 0 && leftBoundedJson[previousIndex] == JsonTokenizerSymbols.EscapeCharacter)
                    continue;

                var targetSpan = leftBoundedJson.Slice(0, i + 1);
                _currentIndex += targetSpan.Length;
                _currentPosition += targetSpan.Length;
                return new JsonTextToken(JsonTokenType.String, targetSpan);
            }

            Throw($"Could not find end of JSON string {leftBoundedJson.ToString()}.");
            return default;
        }

        private void ThrowInvalidConstant(string expectedTokenText, ReadOnlySpan<char> actualTokenText) =>
            Throw($"Expected token \"{expectedTokenText}\" but actually found \"{actualTokenText.ToString()}\" at line {_currentLine} position {_currentPosition}.");

        private static void Throw(string message) =>
            throw new DeserializationException(message);

        public override bool Equals(object obj) =>
            throw new NotSupportedException("ref structs do not support object.Equals as they cannot live on the heap.");

        public override int GetHashCode() =>
            throw new NotSupportedException("ref structs do not support object.GetHashCode as they cannot live on the heap.");

    }
}