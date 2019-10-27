using System;
using Light.GuardClauses;
using Light.Json.FrameworkExtensions;

namespace Light.Json.Tokenization.Utf16
{
    public struct JsonUtf16Tokenizer : IJsonTokenizer<JsonUtf16Token>
    {
        private static readonly string NewLineCharacters = Environment.NewLine;
        private static readonly char NewLineFirstCharacter = NewLineCharacters[0];
        private readonly ReadOnlyMemory<char> _json;
        private int _currentIndex;
        private int _currentLine;
        private int _currentPosition;

        public JsonUtf16Tokenizer(ReadOnlyMemory<char> json)
        {
            _json = json;
            _currentIndex = 0;
            _currentLine = 1;
            _currentPosition = 1;
        }

        public int CurrentIndex => _currentIndex;
        public int CurrentLine => _currentLine;
        public int CurrentPosition => _currentPosition;

        public JsonUtf16Token GetNextToken()
        {
            var json = _json.Span;
            if (!TryReadNextCharacter(json, out var currentCharacter))
                return new JsonUtf16Token(JsonTokenType.EndOfDocument, default, _currentLine, _currentPosition);
            switch (currentCharacter)
            {
                case JsonSymbols.StringDelimiter:
                    return ReadString();
                case JsonSymbols.FalseFirstCharacter:
                    return ReadConstant(json, JsonTokenType.False, JsonSymbols.False);
                case JsonSymbols.TrueFirstCharacter:
                    return ReadConstant(json, JsonTokenType.True, JsonSymbols.True);
                case JsonSymbols.NullFirstCharacter:
                    return ReadConstant(json, JsonTokenType.Null, JsonSymbols.Null);
                case JsonSymbols.MinusSign:
                    return ReadNegativeNumber(json);
                case JsonSymbols.BeginOfObject:
                    return ReadSingleCharacter(JsonTokenType.BeginOfObject);
                case JsonSymbols.EndOfObject:
                    return ReadSingleCharacter(JsonTokenType.EndOfObject);
                case JsonSymbols.BeginOfArray:
                    return ReadSingleCharacter(JsonTokenType.BeginOfArray);
                case JsonSymbols.EndOfArray:
                    return ReadSingleCharacter(JsonTokenType.EndOfArray);
                case JsonSymbols.EntrySeparator:
                    return ReadSingleCharacter(JsonTokenType.EntrySeparator);
                case JsonSymbols.NameValueSeparator:
                    return ReadSingleCharacter(JsonTokenType.NameValueSeparator);
            }

            if (char.IsDigit(currentCharacter))
                return ReadNumber(json);

            throw new DeserializationException($"Unexpected character \"{currentCharacter}\" at line {_currentLine} position {_currentPosition}.");
        }

        private bool TryReadNextCharacter(ReadOnlySpan<char> json, out char currentCharacter)
        {
            // In this method, we search for the first character of the next token.
            var isInSingleLineComment = false;
            while (_currentIndex < json.Length)
            {
                currentCharacter = json[_currentIndex];

                // If the current character is not white space and we are not in a single line comment, then return it
                if (!currentCharacter.IsWhiteSpace() && !isInSingleLineComment)
                {
                    // Check if the character is the beginning of a single line comment.
                    // If not, it can be returned and processed.
                    if (currentCharacter != JsonSymbols.SingleLineCommentCharacter)
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
                    if (currentCharacter != JsonSymbols.SingleLineCommentCharacter)
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

                // If the current Character is not a new line character, advance position and index
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

        private JsonUtf16Token ReadSingleCharacter(JsonTokenType tokenType) =>
            new JsonUtf16Token(tokenType, _json.Slice(_currentIndex++, 1), _currentLine, _currentPosition++);

        private JsonUtf16Token ReadNumber(ReadOnlySpan<char> json)
        {
            int i;
            for (i = _currentIndex + 1; i < json.Length; ++i)
            {
                var currentCharacter = json[i];
                if (char.IsDigit(currentCharacter))
                    continue;

                if (currentCharacter == JsonSymbols.DecimalSymbol)
                    return ReadFloatingPointNumber(json, i);
                break;
            }

            var token = new JsonUtf16Token(JsonTokenType.IntegerNumber, _json.Slice(_currentIndex, i - _currentIndex), _currentLine, _currentPosition);
            _currentIndex = i;
            _currentPosition += token.Length;
            return token;
        }

        private JsonUtf16Token ReadFloatingPointNumber(ReadOnlySpan<char> json, int decimalSymbolIndex)
        {
            int i;
            for (i = decimalSymbolIndex + 1; i < json.Length; ++i)
            {
                if (!char.IsDigit(json[i]))
                    break;
            }

            if (i == decimalSymbolIndex + 1)
            {
                var erroneousToken = GetErroneousToken();
                Throw($"Expected digit after decimal symbol in \"{erroneousToken}\" at line {_currentLine} position {_currentPosition}.");
            }

            var slicedSpan = _json.Slice(_currentIndex, i - _currentIndex);
            var token = new JsonUtf16Token(JsonTokenType.FloatingPointNumber, slicedSpan, _currentLine, _currentPosition);
            _currentIndex = i;
            _currentPosition += slicedSpan.Length;
            return token;
        }

        private string GetErroneousToken()
        {
            var leftBoundJson = _json.Slice(_currentIndex);
            var erroneousToken = leftBoundJson.Slice(0, Math.Min(40, leftBoundJson.Length));
            return erroneousToken.ToString();
        }

        private JsonUtf16Token ReadNegativeNumber(ReadOnlySpan<char> json)
        {
            int i;
            for (i = _currentIndex + 1; i < json.Length; ++i)
            {
                var currentCharacter = json[i];
                if (char.IsDigit(currentCharacter))
                    continue;

                if (currentCharacter == JsonSymbols.DecimalSymbol)
                    return ReadFloatingPointNumber(json, i);
                break;
            }

            if (i == _currentIndex + 1)
                Throw($"Expected digit after minus sign at line {_currentLine} position {_currentPosition}.");

            var token = new JsonUtf16Token(JsonTokenType.IntegerNumber, _json.Slice(_currentIndex, i - _currentIndex), _currentLine, _currentPosition);
            _currentIndex = i;
            _currentPosition += token.Length;
            return token;
        }

        private JsonUtf16Token ReadConstant(ReadOnlySpan<char> json, JsonTokenType type, string expectedSymbol)
        {
            if (_currentIndex + expectedSymbol.Length > _json.Length)
                ThrowInvalidConstant(expectedSymbol, json.Slice(_currentIndex));
            var slicedText = _json.Slice(_currentIndex, expectedSymbol.Length);
            if (!slicedText.Span.Equals(expectedSymbol.AsSpan(), StringComparison.Ordinal))
                ThrowInvalidConstant(expectedSymbol, slicedText.Span);

            var token = new JsonUtf16Token(type, slicedText, _currentLine, _currentPosition);
            _currentIndex += expectedSymbol.Length;
            _currentPosition += expectedSymbol.Length;
            return token;
        }

        private JsonUtf16Token ReadString()
        {
            var leftBoundedJson = _json.Slice(_currentIndex);
            var leftBoundedJsonSpan = leftBoundedJson.Span;
            for (var i = 1; i < leftBoundedJsonSpan.Length; ++i)
            {
                if (leftBoundedJsonSpan[i] != JsonSymbols.StringDelimiter)
                    continue;

                var previousIndex = i - 1;
                if (previousIndex > 0 && leftBoundedJsonSpan[previousIndex] == JsonSymbols.EscapeCharacter)
                    continue;

                var slicedMemory = leftBoundedJson.Slice(0, i + 1);
                var token = new JsonUtf16Token(JsonTokenType.String, slicedMemory, _currentLine, _currentPosition);
                _currentIndex += slicedMemory.Length;
                _currentPosition += slicedMemory.Length;
                return token;
            }

            Throw($"Could not find end of JSON string {leftBoundedJson.ToString()}.");
            return default;
        }

        private void ThrowInvalidConstant(string expectedTokenText, ReadOnlySpan<char> actualTokenText) =>
            Throw($"Expected token \"{expectedTokenText}\" but actually found \"{actualTokenText.ToString()}\" at line {_currentLine} position {_currentPosition}.");

        private static void Throw(string message) =>
            throw new DeserializationException(message);

        public override bool Equals(object obj) =>
            throw BoxingNotSupported.CreateException();

        public override int GetHashCode() =>
            throw BoxingNotSupported.CreateException();
    }
}