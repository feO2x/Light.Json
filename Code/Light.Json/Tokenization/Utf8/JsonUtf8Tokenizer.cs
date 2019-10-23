using System;
using Light.GuardClauses;
using Light.Json.FrameworkExtensions;

namespace Light.Json.Tokenization.Utf8
{
    public struct JsonUtf8Tokenizer
    {
        private static readonly byte[] NewLineCharacters = Environment.NewLine.ToUtf8();
        private static readonly byte NewLineFirstCharacter;
        private readonly ReadOnlyMemory<byte> _jsonInUtf8;
        private int _currentIndex;
        private int _currentLine;
        private int _currentPosition;

        static JsonUtf8Tokenizer()
        {
            Check.InvalidState(NewLineCharacters.Length < 1 || NewLineCharacters.Length > 2, @"The newline characters must be either 1 byte (\n) or 2 bytes long (\r\n).");
            NewLineFirstCharacter = NewLineCharacters[0];
        }

        public JsonUtf8Tokenizer(ReadOnlyMemory<byte> jsonInUtf8)
        {
            _jsonInUtf8 = jsonInUtf8;
            _currentIndex = 0;
            _currentLine = 1;
            _currentPosition = 1;
        }

        public JsonUtf8Token GetNextToken()
        {
            var json = _jsonInUtf8.Span;
            if (!TryReadNextCharacter(json, out var currentCharacter))
                return new JsonUtf8Token(JsonTokenType.EndOfDocument, default, 0, 0, _currentLine, _currentPosition);

            if (currentCharacter == JsonSymbols.StringDelimiter)
                return ReadString(currentCharacter);

            if (currentCharacter.IsDigit())
                return ReadNumber(json);

            if (currentCharacter == JsonSymbols.MinusSign)
                return ReadNegativeNumber(json);

            if (currentCharacter == JsonSymbols.FalseFirstCharacter)
                return ReadConstant(JsonTokenType.False, Utf8Symbols.False);

            if (currentCharacter == JsonSymbols.TrueFirstCharacter)
                return ReadConstant(JsonTokenType.True, Utf8Symbols.True);

            if (currentCharacter == JsonSymbols.NullFirstCharacter)
                return ReadConstant(JsonTokenType.Null, Utf8Symbols.Null);

            if (currentCharacter == JsonSymbols.EntrySeparator)
                return ReadSingleCharacter(JsonTokenType.EntrySeparator, currentCharacter);

            if (currentCharacter == JsonSymbols.NameValueSeparator)
                return ReadSingleCharacter(JsonTokenType.NameValueSeparator, currentCharacter);

            if (currentCharacter == JsonSymbols.BeginOfObject)
                return ReadSingleCharacter(JsonTokenType.BeginOfObject, currentCharacter);

            if (currentCharacter == JsonSymbols.BeginOfArray)
                return ReadSingleCharacter(JsonTokenType.BeginOfArray, currentCharacter);

            if (currentCharacter == JsonSymbols.EndOfObject)
                return ReadSingleCharacter(JsonTokenType.EndOfObject, currentCharacter);

            if (currentCharacter == JsonSymbols.EndOfArray)
                return ReadSingleCharacter(JsonTokenType.EndOfArray, currentCharacter);

            throw new DeserializationException($"Unexpected character \"{currentCharacter.ToString()}\" at line {_currentLine} position {_currentPosition}.");
        }

        private JsonUtf8Token ReadNumber(ReadOnlySpan<byte> json)
        {
            var currentIndex = _currentIndex + 1;
            while (Utf8Character.TryParseNext(json, out var currentCharacter, currentIndex) == Utf8ParseResult.CharacterParsedSuccessfully)
            {
                if (currentCharacter.IsDigit())
                {
                    ++currentIndex;
                    continue;
                }

                if (currentCharacter == JsonSymbols.DecimalSymbol)
                    return ReadFloatingPointNumber(json, currentIndex);
                break;
            }

            var slicedSpan = _jsonInUtf8.Slice(_currentIndex, currentIndex - _currentIndex);
            var token = new JsonUtf8Token(JsonTokenType.IntegerNumber, slicedSpan, slicedSpan.Length, slicedSpan.Length, _currentLine, _currentPosition);
            _currentIndex += token.Memory.Length;
            _currentPosition += token.Memory.Length;
            return token;
        }

        private JsonUtf8Token ReadNegativeNumber(ReadOnlySpan<byte> json)
        {
            var currentIndex = _currentIndex + 1;
            while (Utf8Character.TryParseNext(json, out var currentCharacter, currentIndex) == Utf8ParseResult.CharacterParsedSuccessfully)
            {
                if (currentCharacter.IsDigit())
                {
                    ++currentIndex;
                    continue;
                }

                if (currentCharacter == JsonSymbols.DecimalSymbol)
                    return ReadFloatingPointNumber(json, currentIndex);
                break;
            }

            if (currentIndex == _currentIndex + 1)
                Throw($"Expected digit after minus sign at line {_currentLine} position {_currentPosition}.");

            var slicedSpan = _jsonInUtf8.Slice(_currentIndex, currentIndex - _currentIndex);
            var token = new JsonUtf8Token(JsonTokenType.IntegerNumber, slicedSpan, slicedSpan.Length, slicedSpan.Length, _currentLine, _currentPosition);
            _currentIndex += token.Memory.Length;
            _currentPosition += token.Memory.Length;
            return token;
        }

        private JsonUtf8Token ReadFloatingPointNumber(ReadOnlySpan<byte> json, int decimalSymbolIndex)
        {
            var currentIndex = decimalSymbolIndex + 1;

            while (Utf8Character.TryParseNext(json, out var currentCharacter, currentIndex) == Utf8ParseResult.CharacterParsedSuccessfully)
            {
                if (!currentCharacter.IsDigit())
                    break;

                ++currentIndex;
            }

            if (currentIndex == decimalSymbolIndex + 1)
            {
                var erroneousToken = GetErroneousTokenInUtf16();
                Throw($"Expected digit after decimal symbol in \"{erroneousToken}\" at line {_currentLine} position {_currentPosition}.");
            }

            var slicedSpan = _jsonInUtf8.Slice(_currentIndex, currentIndex - _currentIndex);
            var token = new JsonUtf8Token(JsonTokenType.FloatingPointNumber, slicedSpan, slicedSpan.Length, slicedSpan.Length, _currentLine, _currentPosition);
            _currentIndex += token.Memory.Length;
            _currentPosition += token.Memory.Length;
            return token;
        }

        private JsonUtf8Token ReadString(Utf8Character previousCharacter)
        {
            var leftBoundJson = _jsonInUtf8.Slice(_currentIndex);

            var currentIndex = 1;
            var numberOfCharacters = 1;
            var numberOfChars = 1;
            var leftBoundJsonSpan = leftBoundJson.Span;
            while (Utf8Character.TryParseNext(leftBoundJsonSpan, out var currentCharacter, currentIndex) == Utf8ParseResult.CharacterParsedSuccessfully)
            {
                ++numberOfCharacters;
                numberOfChars += currentCharacter.NumberOfChars;
                if (currentCharacter != JsonSymbols.StringDelimiter ||
                    previousCharacter == JsonSymbols.EscapeCharacter)
                {
                    currentIndex += currentCharacter.ByteLength;
                    previousCharacter = currentCharacter;
                    continue;
                }

                var slicedSpan = leftBoundJson.Slice(0, numberOfCharacters);
                var token = new JsonUtf8Token(JsonTokenType.String, slicedSpan, numberOfCharacters, numberOfChars, _currentLine, _currentPosition);
                _currentIndex += slicedSpan.Length;
                _currentPosition += numberOfCharacters;
                return token;
            }

            Throw($"Could not find end of JSON string starting in line {_currentLine} position {_currentPosition}.");
            return default;
        }

        private JsonUtf8Token ReadConstant(JsonTokenType type, Utf8Constant expectedSymbol)
        {
            if (_currentIndex + expectedSymbol.ByteLength > _jsonInUtf8.Length)
                ThrowInvalidConstant(expectedSymbol);
            var slicedBytes = _jsonInUtf8.Slice(_currentIndex, expectedSymbol.ByteLength);
            if (!slicedBytes.Span.SequenceEqual(expectedSymbol.ByteSequence.Span))
                ThrowInvalidConstant(expectedSymbol);

            var token = new JsonUtf8Token(type, slicedBytes, expectedSymbol.NumberOfCharacters, expectedSymbol.NumberOfCharacters, _currentLine, _currentPosition);
            _currentIndex += expectedSymbol.ByteLength;
            _currentPosition += expectedSymbol.NumberOfCharacters;
            return token;
        }

        private JsonUtf8Token ReadSingleCharacter(JsonTokenType tokenType, Utf8Character character)
        {
            var memory = _jsonInUtf8.Slice(_currentIndex, character.ByteLength);
            var token = new JsonUtf8Token(tokenType, memory, 1, 1, _currentLine, _currentPosition);
            _currentIndex += character.ByteLength;
            ++_currentPosition;
            return token;
        }

        private bool TryReadNextCharacter(ReadOnlySpan<byte> json, out Utf8Character currentCharacter)
        {
            // In this method, we search for the first character of the next token.
            var isInSingleLineComment = false;
            while (_currentIndex < json.Length)
            {
                var parseResult = Utf8Character.TryParseNext(json, out currentCharacter, _currentIndex);
                if (parseResult != Utf8ParseResult.CharacterParsedSuccessfully)
                    throw new DeserializationException($"The UTF8 JSON document could not be decoded because at index {_currentIndex} the UTF8 character is invalid.");

                Utf8Character lookupCharacter;
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
                    parseResult = Utf8Character.TryParseNext(json, out lookupCharacter, _currentIndex + currentCharacter.ByteLength);
                    if (parseResult != Utf8ParseResult.CharacterParsedSuccessfully)
                        return true;

                    // Else check if the next character is actually the second slash of a comment.
                    // If it is not, then return the slash as this will result in an exception
                    // reporting an unexpected character (as above).
                    if (lookupCharacter != JsonSymbols.SingleLineCommentCharacter)
                        return true;

                    // Else we are in a single line comment until we find a new line
                    isInSingleLineComment = true;
                    _currentIndex += 2;
                    _currentPosition += 2;
                }

                // If the current Character is not a new line character, advance position and index
                if (currentCharacter != NewLineFirstCharacter)
                {
                    _currentIndex += currentCharacter.ByteLength;
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

                // Handle \r\n new line characters
                parseResult = Utf8Character.TryParseNext(json, out lookupCharacter, _currentIndex + 1);
                if (parseResult != Utf8ParseResult.CharacterParsedSuccessfully)
                {
                    currentCharacter = default;
                    return false;
                }

                if (lookupCharacter == NewLineCharacters[1])
                {
                    _currentIndex += lookupCharacter.ByteLength + currentCharacter.ByteLength;
                    ++_currentLine;
                    _currentPosition = 1;
                    isInSingleLineComment = false;
                }
            }

            currentCharacter = default;
            return false;
        }

        private static void Throw(string message) =>
            throw new DeserializationException(message);

        private void ThrowInvalidConstant(in Utf8Constant expectedToken)
        {
            var invalidToken = GetErroneousTokenInUtf16();
            if (invalidToken == null)
                throw new DeserializationException($"Expected token \"{expectedToken.Utf16Text}\" at line {_currentLine} position {_currentPosition}.");

            throw new DeserializationException($"Expected token \"{expectedToken.Utf16Text}\" but actually found \"{invalidToken}\" at line {_currentLine} position {_currentPosition}.");
        }

        private string GetErroneousTokenInUtf16()
        {
            var characterArray = new char[40];
            var span = characterArray.AsSpan();
            var currentUtf16Index = 0;

            var currentUtf8Index = _currentIndex;
            var json = _jsonInUtf8.Span;
            while (currentUtf8Index < json.Length)
            {
                var parseResult = Utf8Character.TryParseNext(json, out var utf8Character, currentUtf8Index);
                if (parseResult != Utf8ParseResult.CharacterParsedSuccessfully)
                    break;

                var numberOfUtf16CharactersCopied = utf8Character.CopyUtf16To(span, currentUtf16Index);
                if (numberOfUtf16CharactersCopied == 0)
                    break;

                currentUtf8Index += utf8Character.ByteLength;
                currentUtf16Index += numberOfUtf16CharactersCopied;
            }

            return currentUtf16Index == 0 ? null : new string(characterArray, 0, currentUtf16Index);
        }

        public override bool Equals(object obj) =>
            throw BoxingNotSupported.CreateException();

        public override int GetHashCode() =>
            throw BoxingNotSupported.CreateException();
    }
}