﻿using System;
using System.Text;
using Light.GuardClauses;

namespace Light.Json.Tokenization.Utf8
{
    public ref struct JsonUtf8SpanTokenizer
    {
        private static readonly byte[] NewLineCharacters = Environment.NewLine.ToUtf8();
        private static readonly byte NewLineFirstCharacter;
        private readonly ReadOnlySpan<byte> _json;
        private int _currentIndex;
        private int _currentLine;
        private int _currentPosition;

        static JsonUtf8SpanTokenizer()
        {
            Check.InvalidState(NewLineCharacters.Length < 1 || NewLineCharacters.Length > 2, @"The newline characters must be either 1 byte (\n) or 2 bytes long (\r\n).");
            NewLineFirstCharacter = NewLineCharacters[0];
        }

        public JsonUtf8SpanTokenizer(in ReadOnlySpan<byte> json)
        {
            _json = json;
            _currentIndex = 0;
            _currentLine = 1;
            _currentPosition = 0;
        }

        public JsonUtf8SpanToken GetNextToken()
        {
            if (!TryReadNextCharacter(out var currentCharacter))
                return new JsonUtf8SpanToken(JsonTokenType.EndOfDocument);

            if (currentCharacter == JsonSymbols.StringDelimiter)
                return ReadString(currentCharacter);

            if (currentCharacter == JsonSymbols.FalseFirstCharacter)
                return ReadConstant(JsonTokenType.False, Utf8Symbols.False.Span);

            throw new NotImplementedException();
        }

        private JsonUtf8SpanToken ReadString(Utf8Char previousCharacter)
        {
            var leftBoundJson = _json.Slice(_currentIndex);

            var currentIndex = 1;

            while (Utf8Char.TryParseNext(leftBoundJson, out var currentCharacter, currentIndex) == Utf8ParseResult.CharacterParsedSuccessfully)
            {
                if (currentCharacter != JsonSymbols.StringDelimiter ||
                    previousCharacter == JsonSymbols.EscapeCharacter)
                {
                    currentIndex += currentCharacter.Length;
                    previousCharacter = currentCharacter;
                    continue;
                }

                var slicedSpan = leftBoundJson.Slice(1, currentIndex - 1);
                _currentIndex += slicedSpan.Length + 2;
                _currentPosition += slicedSpan.Length + 2;
                return new JsonUtf8SpanToken(JsonTokenType.String, slicedSpan);
            }

            Throw($"Could not find end of JSON string starting in line {_currentLine} position {_currentPosition}.");
            return default;
        }
        private JsonUtf8SpanToken ReadConstant(JsonTokenType type, in ReadOnlySpan<byte> expectedSymbol)
        {
            if (_currentIndex + expectedSymbol.Length > _json.Length)
                ThrowInvalidConstant(expectedSymbol);
            var slicedBytes = _json.Slice(_currentIndex, expectedSymbol.Length);
            if (!slicedBytes.SequenceEqual(expectedSymbol))
                ThrowInvalidConstant(expectedSymbol);

            _currentIndex += expectedSymbol.Length;
            _currentPosition += expectedSymbol.Length;
            return new JsonUtf8SpanToken(type, slicedBytes);
        }

        private bool TryReadNextCharacter(out Utf8Char currentCharacter)
        {
            // In this method, we search for the first character of the next token.
            var isInSingleLineComment = false;
            while (_currentIndex < _json.Length)
            {
                var parseResult = Utf8Char.TryParseNext(_json, out currentCharacter, _currentIndex);
                if (parseResult != Utf8ParseResult.CharacterParsedSuccessfully)
                    throw new DeserializationException($"The UTF8 JSON document could not be decoded because at position {_currentIndex} the UTF8 character is invalid.");

                Utf8Char lookupCharacter;
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
                    parseResult = Utf8Char.TryParseNext(_json, out lookupCharacter, _currentIndex + currentCharacter.Length);
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

                // Handle new line characters with length 2
                parseResult = Utf8Char.TryParseNext(_json, out lookupCharacter, _currentIndex + 1);
                if (parseResult != Utf8ParseResult.CharacterParsedSuccessfully)
                {
                    currentCharacter = default;
                    return false;
                }

                if (lookupCharacter == NewLineCharacters[1])
                {
                    _currentIndex += 2;
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

        private void ThrowInvalidConstant(in ReadOnlySpan<byte> expectedSymbol) =>
            throw new DeserializationException($"Expected symbol \"{Encoding.UTF8.GetString(expectedSymbol.ToArray())}\" at line {_currentLine} position {_currentPosition}.");
    }
}