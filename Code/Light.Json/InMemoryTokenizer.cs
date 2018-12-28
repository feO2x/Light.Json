using System;

namespace Light.Json
{
    public ref struct InMemoryTokenizer
    {
        private readonly ReadOnlySpan<char> _json;
        private int _currentIndex;

        public InMemoryTokenizer(ReadOnlySpan<char> json)
        {
            _json = json;
            _currentIndex = 0;
        }

        public JsonToken GetNextToken()
        {
            if (_currentIndex >= _json.Length)
                return new JsonToken(JsonTokenType.EndOfDocument);

            var currentCharacter = _json[_currentIndex];
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
            }

            if (char.IsDigit(currentCharacter))
                return ReadNumber();

            throw new NotImplementedException();
        }

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
            }

            var token = new JsonToken(JsonTokenType.IntegerNumber, _json.Slice(_currentIndex, i - _currentIndex));
            _currentIndex = i;
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

            if (i == decimalSymbolIndex)
                throw new DeserializationException("Expected digit after decimal symbol.");

            var token = new JsonToken(JsonTokenType.FloatingPointNumber, _json.Slice(_currentIndex, i - _currentIndex));
            _currentIndex = i;
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
            }

            if (i == _currentIndex + 1)
                throw new DeserializationException("Expected number after minus sign.");

            var token = new JsonToken(JsonTokenType.IntegerNumber, _json.Slice(_currentIndex, i - _currentIndex));
            _currentIndex = i;
            return token;
        }

        private JsonToken ReadConstant(JsonTokenType type, string expectedTokenText)
        {
            var constantTokenText = _json.Slice(_currentIndex, expectedTokenText.Length);
            if (constantTokenText != expectedTokenText.AsSpan())
                throw new DeserializationException($"Expected token \"{expectedTokenText}\" but actually found \"{constantTokenText.ToString()}\".");
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
                return new JsonToken(JsonTokenType.String, targetSpan);
            }

            throw new DeserializationException($"Could not find end of JSON string {leftBoundedJson.ToString()}.");
        }
    }
}