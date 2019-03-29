using System;
using Light.GuardClauses;

namespace Light.Json
{
    public ref struct JsonTextSpanTokenizer
    {
        private static readonly string NewLineCharacters = Environment.NewLine;
        private static readonly char NewLineFirstCharacter = NewLineCharacters[0];
        private readonly ReadOnlySpan<char> _json;
        private int _currentIndex;
        private int _currentLine;
        private int _currentPosition;

        public JsonTextSpanTokenizer(ReadOnlySpan<char> json)
        {
            _json = json;
            _currentIndex = 0;
            _currentLine = 1;
            _currentPosition = 1;
        }

        public JsonTextSpanToken GetNextToken()
        {
            if (!TryReadNextCharacter(out var currentCharacter))
                return new JsonTextSpanToken(JsonTokenType.EndOfDocument);
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

            throw new DeserializationException($"Unexpected character \"{currentCharacter}\" at line {_currentLine} position {_currentPosition}.");
        }

        private bool TryReadNextCharacter(out char currentCharacter)
        {
            // In this method, we search for the first character of the next token.
            var isInSingleLineComment = false;
            while (_currentIndex < _json.Length)
            {
                currentCharacter = _json[_currentIndex];

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
                    currentCharacter = _json[_currentIndex + 1];
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

                currentCharacter = _json[++_currentIndex];
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

        private JsonTextSpanToken ReadSingleCharacter(JsonTokenType tokenType) =>
            new JsonTextSpanToken(tokenType, _json.Slice(_currentIndex++, 1));

        private JsonTextSpanToken ReadNumber()
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

            var token = new JsonTextSpanToken(JsonTokenType.IntegerNumber, _json.Slice(_currentIndex, i - _currentIndex));
            _currentIndex = i;
            _currentPosition += token.Text.Length;
            return token;
        }

        private JsonTextSpanToken ReadFloatingPointNumber(int decimalSymbolIndex)
        {
            int i;
            for (i = decimalSymbolIndex + 1; i < _json.Length; i++)
            {
                if (!char.IsDigit(_json[i]))
                    break;
            }

            var slicedSpan = _json.Slice(_currentIndex, i - _currentIndex);
            if (i == decimalSymbolIndex + 1)
                Throw($"Expected digit after decimal symbol in token \"{slicedSpan.ToString()}\" at line {_currentLine} position {_currentPosition}.");

            var token = new JsonTextSpanToken(JsonTokenType.FloatingPointNumber, slicedSpan);
            _currentIndex = i;
            _currentPosition += slicedSpan.Length;
            return token;
        }

        private JsonTextSpanToken ReadNegativeNumber()
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

            var token = new JsonTextSpanToken(JsonTokenType.IntegerNumber, _json.Slice(_currentIndex, i - _currentIndex));
            _currentIndex = i;
            _currentPosition += token.Text.Length;
            return token;
        }

        private JsonTextSpanToken ReadConstant(JsonTokenType type, string expectedTokenText)
        {
            if (_currentIndex + expectedTokenText.Length > _json.Length)
                ThrowInvalidConstant(expectedTokenText, _json.Slice(_currentIndex));
            var slicedText = _json.Slice(_currentIndex, expectedTokenText.Length);
            if (!slicedText.Equals(expectedTokenText.AsSpan(), StringComparison.Ordinal))
                ThrowInvalidConstant(expectedTokenText, slicedText);
            _currentIndex += expectedTokenText.Length;
            return new JsonTextSpanToken(type, slicedText);
        }

        private JsonTextSpanToken ReadString()
        {
            var leftBoundedJson = _json.Slice(_currentIndex);

            for (var i = 1; i < leftBoundedJson.Length; ++i)
            {
                if (leftBoundedJson[i] != JsonTokenizerSymbols.StringDelimiter)
                    continue;

                var previousIndex = i - 1;
                if (previousIndex > 0 && leftBoundedJson[previousIndex] == JsonTokenizerSymbols.EscapeCharacter)
                    continue;

                var slicedSpan = leftBoundedJson.Slice(1, i - 1);
                _currentIndex += slicedSpan.Length + 2;
                _currentPosition += slicedSpan.Length + 2;
                return new JsonTextSpanToken(JsonTokenType.String, slicedSpan);
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