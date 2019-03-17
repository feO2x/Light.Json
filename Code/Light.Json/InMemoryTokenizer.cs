using System;
using Light.GuardClauses;

namespace Light.Json
{
    public ref struct InMemoryTokenizer
    {
        private static readonly string NewLineCharacters = Environment.NewLine;
        private static readonly char NewLineFirstCharacter = NewLineCharacters[0];
        private readonly ReadOnlySpan<char> _json;
        private int _currentIndex;
        private int _currentLine;
        private int _currentPosition;

        public InMemoryTokenizer(ReadOnlySpan<char> json)
        {
            _json = json;
            _currentIndex = 0;
            _currentLine = 1;
            _currentPosition = 1;
        }

        public int CurrentIndex => _currentIndex;

        public int CurrentLine => _currentLine;

        public JsonToken GetNextToken()
        {
            if (!TryReadNextCharacter(out var currentCharacter))
                return new JsonToken(JsonTokenType.EndOfDocument);
            switch (currentCharacter)
            {
                case JsonTokenizerSymbols.StringDelimiter:
                    return ReadString();
                case JsonTokenizerSymbols.FalseStartCharacter:
                    return ReadConstant(JsonTokenType.False, JsonTokenizerSymbols.False);
                case JsonTokenizerSymbols.TrueStartCharacter:
                    return ReadConstant(JsonTokenType.True, JsonTokenizerSymbols.True);
                case JsonTokenizerSymbols.NullStartCharacter:
                    return ReadConstant(JsonTokenType.Null, JsonTokenizerSymbols.Null);
                case JsonTokenizerSymbols.MinusSign:
                    return ReadNegativeNumber();
                case JsonTokenizerSymbols.BeginOfObject:
                    return ReadSingleCharacter(JsonTokenType.BeginOfObject);
                case JsonTokenizerSymbols.EndOfObject:
                    return ReadSingleCharacter(JsonTokenType.EndOfObject);
                case JsonTokenizerSymbols.BeginOfArray:
                    return ReadSingleCharacter(JsonTokenType.BeginOfArray);
                case JsonTokenizerSymbols.EndOfArray:
                    return ReadSingleCharacter(JsonTokenType.EndOfArray);
                case JsonTokenizerSymbols.EntrySeparator:
                    return ReadSingleCharacter(JsonTokenType.EntrySeparator);
                case JsonTokenizerSymbols.NameValueSeparator:
                    return ReadSingleCharacter(JsonTokenType.NameValueSeparator);
            }

            if (char.IsDigit(currentCharacter))
                return ReadNumber();

            throw new NotImplementedException();
        }

        private bool TryReadNextCharacter(out char currentCharacter)
        {
            // Read until the end of the document is reached
            while (_currentIndex < _json.Length)
            {
                currentCharacter = _json[_currentIndex];

                // If the current character is not white space, then return it
                if (!currentCharacter.IsWhiteSpace())
                    return true;

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
                    continue;
                }

                // Check if there is enough space for a second line character
                if (_currentIndex + 2 >= _json.Length)
                {
                    currentCharacter = default;
                    return false;
                }

                currentCharacter = _json[++_currentIndex];
                ++_currentPosition;
                if (currentCharacter == NewLineCharacters[1])
                {
                    ++_currentIndex;
                    ++_currentLine;
                    _currentPosition = 1;
                }
            }

            currentCharacter = default;
            return false;
        }

        private JsonToken ReadSingleCharacter(JsonTokenType tokenType) =>
            new JsonToken(tokenType, _json.Slice(_currentIndex++, 1));

        private JsonToken ReadNumber()
        {
            int i;
            for (i = _currentIndex + 1; i < _json.Length; ++i)
            {
                var currentCharacter = _json[i];
                if (char.IsDigit(currentCharacter))
                    continue;

                if (currentCharacter == JsonTokenizerSymbols.DecimalSymbol)
                    return ReadFloatingPointNumber(i);
                break;
            }

            var token = new JsonToken(JsonTokenType.IntegerNumber, _json.Slice(_currentIndex, i - _currentIndex));
            _currentIndex = i;
            _currentPosition += token.Text.Length;
            return token;
        }

        private JsonToken ReadFloatingPointNumber(int decimalSymbolIndex)
        {
            int i;
            for (i = decimalSymbolIndex + 1; i < _json.Length; i++)
            {
                if (!char.IsDigit(_json[i]))
                    break;
            }

            var textSpan = _json.Slice(_currentIndex, i - _currentIndex);
            if (i == decimalSymbolIndex + 1)
                Throw($"Expected digit after decimal symbol in token \"{textSpan.ToString()}\" at line {_currentLine} position {_currentPosition}.");

            var token = new JsonToken(JsonTokenType.FloatingPointNumber, textSpan);
            _currentIndex = i;
            _currentPosition += token.Text.Length;
            return token;
        }

        private JsonToken ReadNegativeNumber()
        {
            int i;
            for (i = _currentIndex + 1; i < _json.Length; ++i)
            {
                var currentCharacter = _json[i];
                if (char.IsDigit(currentCharacter))
                    continue;

                if (currentCharacter == JsonTokenizerSymbols.DecimalSymbol)
                    return ReadFloatingPointNumber(i);
                break;
            }

            if (i == _currentIndex + 1)
                Throw($"Expected digit after minus sign at line {_currentLine} position {_currentPosition}.");

            var token = new JsonToken(JsonTokenType.IntegerNumber, _json.Slice(_currentIndex, i - _currentIndex));
            _currentIndex = i;
            _currentPosition += token.Text.Length;
            return token;
        }

        private JsonToken ReadConstant(JsonTokenType type, string expectedTokenText)
        {
            if (_currentIndex + expectedTokenText.Length > _json.Length)
                ThrowInvalidConstant(expectedTokenText, _json.Slice(_currentIndex));
            var constantTokenText = _json.Slice(_currentIndex, expectedTokenText.Length);
            if (!constantTokenText.Equals(expectedTokenText.AsSpan(), StringComparison.Ordinal))
                ThrowInvalidConstant(expectedTokenText, constantTokenText);
            _currentIndex += expectedTokenText.Length;
            return new JsonToken(type, constantTokenText);
        }

        private JsonToken ReadString()
        {
            var leftBoundedJson = _json.Slice(_currentIndex);

            for (var i = 1; i < leftBoundedJson.Length; ++i)
            {
                if (leftBoundedJson[i] != JsonTokenizerSymbols.StringDelimiter)
                    continue;

                var targetSpan = leftBoundedJson.Slice(0, i + 1);
                _currentIndex += targetSpan.Length;
                _currentPosition += targetSpan.Length;
                return new JsonToken(JsonTokenType.String, targetSpan);
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