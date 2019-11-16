using System;
using Light.GuardClauses;
using Light.Json.FrameworkExtensions;

namespace Light.Json.Tokenization.Utf8
{
    public struct JsonUtf8Tokenizer : IJsonTokenizer<JsonUtf8Token>
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

        public string ReadString()
        {
            var json = _jsonInUtf8.Span;
            if (!TrySkipWhiteSpace(json))
                throw new DeserializationException($"Expected JSON string at line {_currentLine} position {_currentPosition}.");
            if (json[_currentIndex] != JsonSymbols.QuotationMark)
                throw new DeserializationException($"Expected JSON string at line {_currentLine} position {_currentPosition} (near \"{GetErroneousTokenInUtf16()}\").");

            for (var i = _currentIndex + 1; i < json.Length; ++i)
            {
                switch (json[i])
                {
                    case (byte) JsonSymbols.QuotationMark:
                        var targetSpan = json.Slice(_currentIndex + 1, i - _currentIndex - 1);
                        _currentIndex += targetSpan.Length;

                        var targetString = targetSpan.ConvertFromUtf8ToString();

                        _currentPosition += targetString.Length + 2;
                        return targetString;
                    case (byte) JsonSymbols.Backslash:
                        return ReadStringWithEscapeSequences(json, i);
                }
            }

            throw CreateEndOfJsonStringNotFoundException();
        }

        private DeserializationException CreateEndOfJsonStringNotFoundException() =>
            new DeserializationException($"Could not find end of JSON string \"{GetErroneousTokenInUtf16()}\" at line {_currentLine} position {_currentPosition}.");

        private string ReadStringWithEscapeSequences(in ReadOnlySpan<byte> json, int indexOfFirstEscapeCharacter)
        {
            // When this method is called, we are at the beginning of an escape sequence.
            // We first read to the end of the string and count how many characters need to be escaped.
            var byteLength = DetermineByteLengthOfStringWithEscapeSequences(json, indexOfFirstEscapeCharacter + 1);

            // Then we get hold of a new byte array that we use to escape the corresponding characters
            // TODO: get hold of a pre-allocated byte array on the heap if the byteLength exceeds a certain threshold
            Span<byte> escapedArray = stackalloc byte[byteLength];

            // Copy the characters from the beginning to the first byte sequence to the target array
            var currentIndex = 0;
            if (indexOfFirstEscapeCharacter > _currentIndex + 1)
            {
                var firstUnescapedCharacters = json.Slice(_currentIndex + 1, indexOfFirstEscapeCharacter - (_currentIndex + 1));
                firstUnescapedCharacters.CopyTo(escapedArray);
                currentIndex += firstUnescapedCharacters.Length;
            }

            // Copy all remaining characters, escape where necessary
            var i = indexOfFirstEscapeCharacter + 1;
            var isInEscapeSequence = true;
            while (currentIndex < escapedArray.Length)
            {
                var currentCharacter = json[i];
                if (!isInEscapeSequence)
                {
                    switch (currentCharacter)
                    {
                        case (byte) JsonSymbols.Backslash:
                            isInEscapeSequence = true;
                            ++i;
                            continue;

                        default:
                            escapedArray[currentIndex++] = currentCharacter;
                            ++i;
                            continue;
                    }
                }

                EscapeAndCopyToTargetSpan(json, ref i, escapedArray, ref currentIndex);
                isInEscapeSequence = false;
            }

            return escapedArray.ConvertFromUtf8ToString(escapedArray.Length);
        }

        private void EscapeAndCopyToTargetSpan(in ReadOnlySpan<byte> json, ref int i, in Span<byte> escapedArray, ref int currentIndex)
        {
            var currentCharacter = json[i++];
            switch (currentCharacter)
            {
                case (byte) JsonSymbols.QuotationMark:
                    escapedArray[currentIndex++] = (byte) '"';
                    return;
                case (byte) JsonSymbols.Backslash:
                    escapedArray[currentIndex++] = (byte) '\\';
                    return;
                case (byte) JsonSymbols.Slash:
                    escapedArray[currentIndex++] = (byte) '/';
                    return;
                case (byte) 'b':
                    escapedArray[currentIndex++] = (byte) '\b';
                    return;
                case (byte) 'f':
                    escapedArray[currentIndex++] = (byte) '\f';
                    return;
                case (byte) 'n':
                    escapedArray[currentIndex++] = (byte) '\n';
                    return;
                case (byte) 'r':
                    escapedArray[currentIndex++] = (byte) '\r';
                    return;
                case (byte) 't':
                    escapedArray[currentIndex++] = (byte) '\t';
                    return;
                default: // only an hex unicode escape sequence is left
                    var utf16Value = ConvertHexadecimalJsonCharacterToInt32(json[i]) * 4096;
                    utf16Value += ConvertHexadecimalJsonCharacterToInt32(json[i + 1]) * 256;
                    utf16Value += ConvertHexadecimalJsonCharacterToInt32(json[i + 2]) * 16;
                    utf16Value += ConvertHexadecimalJsonCharacterToInt32(json[i + 3]);
                    i += 4;

                    if (utf16Value < 128)
                    {
                        escapedArray[currentIndex++] = (byte) utf16Value;
                        return;
                    }

                    byte utf8Byte;
                    if (utf16Value < 2048)
                    {
                        // Two-Byte-UTF8-Sequence: 110xxxxx 10xxxxxx
                        utf8Byte = 0b_110_00000;
                        utf8Byte &= (byte) ((utf16Value >> 6) & 0b_11111);
                        escapedArray[currentIndex++] = utf8Byte;
                        utf8Byte = 0b_10_000000;
                        utf8Byte &= (byte) (utf16Value & 0b_111111);
                        escapedArray[currentIndex++] = utf8Byte;
                        return;
                    }

                    if (utf16Value < 65536)
                    {
                        // Three-Byte-UTF8-Sequence: 1110xxxx 10xxxxxx 10xxxxxx
                        utf8Byte = 0b_1110_0000;
                        utf8Byte &= (byte) ((utf16Value >> 12) & 0b_1111);
                        escapedArray[currentIndex++] = utf8Byte;
                        utf8Byte = 0b_10_000000;
                        utf8Byte &= (byte) ((utf16Value >> 6) & 0b_111111);
                        escapedArray[currentIndex++] = utf8Byte;
                        utf8Byte = 0b_10_000000;
                        utf8Byte &= (byte) (utf16Value & 0b_111111);
                        escapedArray[currentIndex++] = utf8Byte;
                        return;
                    }

                    // Else it's a Four-Byte-UTF8-Sequence: 11110xxx 10xxxxxx 10xxxxxx 10xxxxxx
                    utf8Byte = 0b_11110_000;
                    utf8Byte &= (byte) ((utf16Value >> 18) & 0b_111);
                    escapedArray[currentIndex++] = utf8Byte;
                    utf8Byte = 0b_10_000000;
                    utf8Byte &= (byte) ((utf16Value >> 12) & 0b_111111);
                    escapedArray[currentIndex++] = utf8Byte;
                    utf8Byte = 0b_10_000000;
                    utf8Byte &= (byte) ((utf16Value >> 6) & 0b_111111);
                    escapedArray[currentIndex++] = utf8Byte;
                    utf8Byte = 0b_10_000000;
                    utf8Byte &= (byte) (utf16Value & 0b_111111);
                    escapedArray[currentIndex++] = utf8Byte;
                    return;
            }
        }

        private int DetermineByteLengthOfStringWithEscapeSequences(in ReadOnlySpan<byte> json, int i)
        {
            var deltaBytes = 0;
            var isInEscapeSequence = true;
            for (; i < json.Length; ++i)
            {
                var currentByte = json[i];
                if (isInEscapeSequence)
                {
                    deltaBytes -= DetermineByteLengthDeltaOfEscapedCharacter(json, currentByte, ref i);
                    isInEscapeSequence = false;
                    continue;
                }

                switch (currentByte)
                {
                    case (byte) JsonSymbols.QuotationMark:
                        /* The resulting string will not contain any quotation marks,
                         * thus, we will subtract 1 (one quotation mark is removed already
                         * by calculating with the indexes only).
                         */
                        return i - _currentIndex + deltaBytes - 1;
                    case (byte) JsonSymbols.Backslash:
                        isInEscapeSequence = true;
                        continue;
                }
            }

            throw CreateEndOfJsonStringNotFoundException();
        }

        private int DetermineByteLengthDeltaOfEscapedCharacter(in ReadOnlySpan<byte> json, byte secondByteInEscapeSequence, ref int currentIndex)
        {
            switch (secondByteInEscapeSequence)
            {
                case (byte) JsonSymbols.QuotationMark:
                case (byte) JsonSymbols.Backslash:
                case (byte) JsonSymbols.Slash:
                case (byte) 'b':
                case (byte) 'f':
                case (byte) 'n':
                case (byte) 'r':
                case (byte) 't':
                    ++currentIndex;
                    return 1;
                case (byte) 'u':
                    currentIndex += 4;
                    if (currentIndex >= json.Length)
                        throw CreateInvalidEscapeSequenceInJsonStringException();
                    int value = ConvertHexadecimalJsonCharacterToInt32(json[currentIndex - 3]) * 4096; // 16 * 16 * 16 = 4096
                    value += ConvertHexadecimalJsonCharacterToInt32(json[currentIndex - 2]) * 256; // 16 * 16 = 256
                    value += ConvertHexadecimalJsonCharacterToInt32(json[currentIndex - 1]) * 16;
                    value += ConvertHexadecimalJsonCharacterToInt32(json[currentIndex++]);
                    if (value < 128)
                        return 5;
                    if (value < 2048)
                        return 4;
                    if (value < 65536)
                        return 3;
                    return 2;
            }

            throw CreateInvalidEscapeSequenceInJsonStringException();
        }

        private int ConvertHexadecimalJsonCharacterToInt32(byte character)
        {
            switch (character)
            {
                case (byte) '0': return 0;
                case (byte) '1': return 1;
                case (byte) '2': return 2;
                case (byte) '3': return 3;
                case (byte) '4': return 4;
                case (byte) '5': return 5;
                case (byte) '6': return 6;
                case (byte) '7': return 7;
                case (byte) '8': return 8;
                case (byte) '9': return 9;
                case (byte) 'a':
                case (byte) 'A': return 10;
                case (byte) 'b':
                case (byte) 'B': return 11;
                case (byte) 'c':
                case (byte) 'C': return 12;
                case (byte) 'd':
                case (byte) 'D': return 13;
                case (byte) 'e':
                case (byte) 'E': return 14;
                case (byte) 'f':
                case (byte) 'F': return 15;
            }

            throw CreateInvalidEscapeSequenceInJsonStringException();
        }

        private DeserializationException CreateInvalidEscapeSequenceInJsonStringException() =>
            new DeserializationException($"Found invalid escape sequence in JSON string \"{GetErroneousTokenInUtf16()}\" at line {_currentLine} position {_currentPosition}.");

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

        private JsonUtf8Token ReadStringToken(Utf8Character previousCharacter)
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
                if (currentCharacter != JsonSymbols.QuotationMark ||
                    previousCharacter == JsonSymbols.Backslash)
                {
                    currentIndex += currentCharacter.ByteLength;
                    previousCharacter = currentCharacter;
                    continue;
                }

                var slicedMemory = leftBoundJson.Slice(0, numberOfCharacters);
                var token = new JsonUtf8Token(JsonTokenType.String, slicedMemory, numberOfCharacters, numberOfChars, _currentLine, _currentPosition);
                _currentIndex += slicedMemory.Length;
                _currentPosition += numberOfCharacters;
                return token;
            }

            throw new DeserializationException($"Could not find end of JSON string starting in line {_currentLine} position {_currentPosition}.");
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
            var invalidToken = GetErroneousTokenInUtf16();
            if (invalidToken == null)
                throw new DeserializationException($"Expected token \"{expectedToken.Utf16Text}\" at line {_currentLine} position {_currentPosition}.");

            throw new DeserializationException($"Expected token \"{expectedToken.Utf16Text}\" but actually found \"{invalidToken}\" at line {_currentLine} position {_currentPosition}.");
        }

        private string GetErroneousTokenInUtf16()
        {
            var jsonSpan = _jsonInUtf8.Slice(_currentIndex).Span;
            for (var i = 0; i < jsonSpan.Length; ++i)
            {
                if (jsonSpan[i] != (byte) JsonSymbols.LineFeed)
                    continue;

                var length = i;
                if (i - 1 > 0 && jsonSpan[i - 1] == JsonSymbols.CarriageReturn)
                    --length;

                return jsonSpan.ConvertFromUtf8ToString(length);
            }

            if (jsonSpan.Length > 40)
                jsonSpan = jsonSpan.Slice(0, 40);

            return jsonSpan.ConvertFromUtf8ToString();
        }

        public override bool Equals(object obj) =>
            throw BoxingNotSupported.CreateException();

        public override int GetHashCode() =>
            throw BoxingNotSupported.CreateException();
    }
}