using System;
using Light.GuardClauses;
using Light.Json.FrameworkExtensions;

namespace Light.Json.Tokenization.Utf8
{
    public partial struct JsonUtf8Tokenizer : IJsonTokenizer<JsonUtf8Token>
    {
        private static readonly byte[] NewLineCharacters = Environment.NewLine.ToUtf8();
        private static readonly byte NewLineFirstCharacter;
        private readonly ReadOnlyMemory<byte> _json;
        private int _currentIndex;
        private int _currentLine;
        private int _currentPosition;

        static JsonUtf8Tokenizer()
        {
            Check.InvalidState(NewLineCharacters.Length < 1 || NewLineCharacters.Length > 2, @"The newline characters must be either 1 byte (\n) or 2 bytes long (\r\n).");
            NewLineFirstCharacter = NewLineCharacters[0];
        }

        public JsonUtf8Tokenizer(ReadOnlyMemory<byte> json)
        {
            _json = json;
            _currentIndex = 0;
            _currentLine = 1;
            _currentPosition = 1;
        }

        public JsonUtf8Token GetNextToken()
        {
            var json = _json.Span;
            if (!TryReadNextCharacter(json, out var currentCharacter))
                return new JsonUtf8Token(JsonTokenType.EndOfDocument, default, 0, 0, _currentLine, _currentPosition);

            if (currentCharacter == JsonSymbols.QuotationMark)
                return ReadStringToken(currentCharacter);

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

            var slicedSpan = _json.Slice(_currentIndex, currentIndex - _currentIndex);
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

            var slicedSpan = _json.Slice(_currentIndex, currentIndex - _currentIndex);
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
                var erroneousToken = GetErroneousToken();
                Throw($"Expected digit after decimal symbol in \"{erroneousToken}\" at line {_currentLine} position {_currentPosition}.");
            }

            var slicedSpan = _json.Slice(_currentIndex, currentIndex - _currentIndex);
            var token = new JsonUtf8Token(JsonTokenType.FloatingPointNumber, slicedSpan, slicedSpan.Length, slicedSpan.Length, _currentLine, _currentPosition);
            _currentIndex += token.Memory.Length;
            _currentPosition += token.Memory.Length;
            return token;
        }

        private JsonUtf8Token ReadConstant(JsonTokenType type, Utf8Constant expectedSymbol)
        {
            if (_currentIndex + expectedSymbol.ByteLength > _json.Length)
                ThrowInvalidConstant(expectedSymbol);
            var slicedBytes = _json.Slice(_currentIndex, expectedSymbol.ByteLength);
            if (!slicedBytes.Span.SequenceEqual(expectedSymbol.ByteSequence.Span))
                ThrowInvalidConstant(expectedSymbol);

            var token = new JsonUtf8Token(type, slicedBytes, expectedSymbol.NumberOfCharacters, expectedSymbol.NumberOfCharacters, _currentLine, _currentPosition);
            _currentIndex += expectedSymbol.ByteLength;
            _currentPosition += expectedSymbol.NumberOfCharacters;
            return token;
        }

        private JsonUtf8Token ReadSingleCharacter(JsonTokenType tokenType, Utf8Character character)
        {
            var memory = _json.Slice(_currentIndex, character.ByteLength);
            var token = new JsonUtf8Token(tokenType, memory, 1, 1, _currentLine, _currentPosition);
            _currentIndex += character.ByteLength;
            ++_currentPosition;
            return token;
        }

        private bool TrySkipWhiteSpace(ReadOnlySpan<byte> json)
        {
            for (var i = _currentIndex; i < json.Length; ++i)
            {
                var currentByte = json[i];
                switch (currentByte)
                {
                    case (byte) JsonSymbols.Space:
                    case (byte) JsonSymbols.HorizontalTab:
                    case (byte) JsonSymbols.CarriageReturn:
                        ++_currentPosition;
                        continue;
                    case (byte) JsonSymbols.LineFeed:
                        _currentPosition = 1;
                        ++_currentLine;
                        continue;
                    default:
                        _currentIndex += i;
                        return true;
                }
            }

            _currentIndex = json.Length;
            return false;
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
            var invalidToken = GetErroneousToken();
            if (invalidToken == null)
                throw new DeserializationException($"Expected token \"{expectedToken.Utf16Text}\" at line {_currentLine} position {_currentPosition}.");

            throw new DeserializationException($"Expected token \"{expectedToken.Utf16Text}\" but actually found \"{invalidToken}\" at line {_currentLine} position {_currentPosition}.");
        }

        private string GetErroneousToken()
        {
            var remainingJsonLength = _json.Length - _currentIndex;
            var json = _json.Slice(_currentIndex, Math.Min(remainingJsonLength, 40)).Span;

            if (json.IsEmpty)
                return "";

            if (json[0] == '"')
                return GetErroneousJsonStringToken(json);

            for (var i = 0; i < json.Length; ++i)
            {
                switch (json[i])
                {
                    case (byte) '\n':
                        var length = i;
                        if (i - 1 > 0 && json[i - 1] == '\r')
                            --length;
                        return json.ConvertFromUtf8ToString(length);
                    case (byte) ',':
                    case (byte) ':':
                    case (byte) ']':
                    case (byte) '}':
                        return json.ConvertFromUtf8ToString(i);
                }
            }

            return json.ConvertFromUtf8ToString();
        }

        private static string GetErroneousJsonStringToken(in ReadOnlySpan<byte> utf8Json)
        {
            byte previousCharacter = 0;
            for (var i = 1; i < utf8Json.Length; ++i)
            {
                var currentCharacter = utf8Json[i];
                if (currentCharacter == '"' && previousCharacter != '\\')
                    return utf8Json.ConvertFromUtf8ToString(Math.Min(utf8Json.Length, i + 1));

                previousCharacter = currentCharacter;
            }

            return utf8Json.ConvertFromUtf8ToString();
        }

        public override bool Equals(object obj) =>
            throw BoxingNotSupported.CreateException();

        public override int GetHashCode() =>
            throw BoxingNotSupported.CreateException();
    }
}