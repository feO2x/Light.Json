using System;
using Light.Json.FrameworkExtensions;

namespace Light.Json.Deserialization.Tokenization.Utf8
{
    public partial struct JsonUtf8Tokenizer
    {
        public string ReadString()
        {
            var json = _json.Span;
            if (!TrySkipWhiteSpace(json))
                throw new DeserializationException("Expected JSON string but found end of document.");
            if (json[CurrentIndex] != '"')
                throw new DeserializationException($"Expected JSON string at line {CurrentLine} position {CurrentPosition} (near \"{GetErroneousToken()}\").");

            for (var i = CurrentIndex + 1; i < json.Length; ++i)
            {
                switch (json[i])
                {
                    case (byte) '"':
                        var targetSpan = json.Slice(CurrentIndex + 1, i - CurrentIndex - 1);
                        var targetString = targetSpan.ConvertFromUtf8ToString();

                        CurrentIndex += targetSpan.Length + 2;
                        CurrentPosition += targetString.Length + 2;
                        return targetString;
                    case (byte) '\\':
                        return ReadStringWithEscapeSequences(json, i);
                }
            }

            throw CreateEndOfJsonStringNotFoundException();
        }

        public JsonUtf8Token ReadNameToken()
        {
            var token = GetNextToken();
            token.MustBeOfType(JsonTokenType.String);
            ReadNameValueSeparator();
            return token;
        }

        private JsonUtf8Token ReadStringToken()
        {
            var leftBoundedJson = _json.Slice(CurrentIndex);
            var leftBoundedJsonSpan = leftBoundedJson.Span;
            for (var i = 1; i < leftBoundedJsonSpan.Length; ++i)
            {
                if (leftBoundedJsonSpan[i] != JsonSymbols.QuotationMark)
                    continue;

                var previousIndex = i - 1;
                if (previousIndex > 0 && leftBoundedJsonSpan[previousIndex] == JsonSymbols.Backslash)
                    continue;

                var slicedMemory = leftBoundedJson.Slice(0, i + 1);
                var token = new JsonUtf8Token(JsonTokenType.String, slicedMemory, CurrentLine, CurrentPosition);
                CurrentIndex += slicedMemory.Length;
                CurrentPosition += slicedMemory.Length;
                return token;
            }

            throw CreateEndOfJsonStringNotFoundException();
        }

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
            if (indexOfFirstEscapeCharacter > CurrentIndex + 1)
            {
                var firstUnescapedCharacters = json.Slice(CurrentIndex + 1, indexOfFirstEscapeCharacter - (CurrentIndex + 1));
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
                        return i - CurrentIndex + deltaBytes - 1;
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

        private DeserializationException CreateEndOfJsonStringNotFoundException() =>
            new DeserializationException($"Could not find end of JSON string \"{GetErroneousToken()}\" at line {CurrentLine} position {CurrentPosition}.");

        private DeserializationException CreateInvalidEscapeSequenceInJsonStringException() =>
            new DeserializationException($"Found invalid escape sequence in JSON string \"{GetErroneousToken()}\" at line {CurrentLine} position {CurrentPosition}.");
    }
}