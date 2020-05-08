using System;
using System.Runtime.Serialization;

namespace Light.Json.Deserialization.Tokenization.Utf16
{
    public partial struct JsonUtf16Tokenizer
    {
        public string ReadString()
        {
            var json = _json.Span;
            if (!TrySkipWhiteSpace(json))
                throw new SerializationException("Expected JSON string but found end of document.");
            if (json[CurrentIndex] != JsonSymbols.QuotationMark)
                throw new SerializationException($"Expected JSON string at line {CurrentLine} position {CurrentPosition} (near \"{GetErroneousToken()}\").");

            for (var i = CurrentIndex + 1; i < json.Length; ++i)
            {
                switch (json[i])
                {
                    case JsonSymbols.QuotationMark:
                        var targetSpan = json.Slice(CurrentIndex + 1, i - CurrentIndex - 1);
                        var targetString = targetSpan.ToString();
                        CurrentIndex += targetSpan.Length + 2;
                        CurrentPosition += targetString.Length + 2;
                        return targetString;
                    case JsonSymbols.Backslash:
                        return ReadStringWithEscapeSequences(json, i);
                }
            }

            throw CreateEndOfJsonStringNotFoundException();
        }

        public JsonUtf16Token ReadNameToken()
        {
            var token = GetNextToken();
            token.MustBeOfType(JsonTokenType.String);
            ReadNameValueSeparator();
            return token;
        }

        private JsonUtf16Token ReadStringToken()
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
                var token = new JsonUtf16Token(JsonTokenType.String, slicedMemory, CurrentLine, CurrentPosition);
                CurrentIndex += slicedMemory.Length;
                CurrentPosition += slicedMemory.Length;
                return token;
            }

            throw CreateEndOfJsonStringNotFoundException();
        }

        private string ReadStringWithEscapeSequences(in ReadOnlySpan<char> json, int indexOfFirstEscapeCharacter)
        {
            /* TODO: this method could be optimized.
             * We should check if stackalloc makes sense by looking forward for 128 or so chars and check if we could find
             * the end of the JSON string. If a certain threshold is not hit, we could perform a stackalloc for the escaped
             * string and not suffer the thread synchronization costs of ArrayPool<T>. If the JSON string to be escaped is
             * longer, then stackalloc should not be performed to avoid a stack overflow.
             */

            // When this method is called, we are at the beginning of an escape sequence.
            // We first read to the end of the string and count how many characters need to be escaped.
            var length = DetermineByteLengthOfStringWithEscapeSequences(json, indexOfFirstEscapeCharacter + 1);

            // Then we get hold of a new byte array that we use to escape the corresponding characters
            // TODO: get hold of a pre-allocated byte array on the heap if the byteLength exceeds a certain threshold
            Span<char> escapedArray = stackalloc char[length];

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
                        case JsonSymbols.Backslash:
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

            return escapedArray.ToString();
        }

        private void EscapeAndCopyToTargetSpan(in ReadOnlySpan<char> json, ref int i, in Span<char> escapedArray, ref int currentIndex)
        {
            var currentCharacter = json[i++];
            switch (currentCharacter)
            {
                case JsonSymbols.QuotationMark:
                    escapedArray[currentIndex++] = '"';
                    return;
                case JsonSymbols.Backslash:
                    escapedArray[currentIndex++] = '\\';
                    return;
                case JsonSymbols.Slash:
                    escapedArray[currentIndex++] = '/';
                    return;
                case 'b':
                    escapedArray[currentIndex++] = '\b';
                    return;
                case 'f':
                    escapedArray[currentIndex++] = '\f';
                    return;
                case 'n':
                    escapedArray[currentIndex++] = '\n';
                    return;
                case 'r':
                    escapedArray[currentIndex++] = '\r';
                    return;
                case 't':
                    escapedArray[currentIndex++] = '\t';
                    return;
                default: // only an hex unicode escape sequence is left
                    var utf16Value = ConvertHexadecimalJsonCharacterToInt32(json[i]) * 4096;
                    utf16Value += ConvertHexadecimalJsonCharacterToInt32(json[i + 1]) * 256;
                    utf16Value += ConvertHexadecimalJsonCharacterToInt32(json[i + 2]) * 16;
                    utf16Value += ConvertHexadecimalJsonCharacterToInt32(json[i + 3]);
                    i += 4;

                    if (utf16Value <= char.MaxValue)
                    {
                        escapedArray[currentIndex++] = (char) utf16Value;
                        return;
                    }

                    escapedArray[currentIndex++] = (char) (utf16Value >> 16);
                    escapedArray[currentIndex++] = (char) (utf16Value & 0xFFFF);
                    return;
            }
        }

        private int DetermineByteLengthOfStringWithEscapeSequences(in ReadOnlySpan<char> json, int i)
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
                    case JsonSymbols.QuotationMark:
                        /* The resulting string will not contain any quotation marks,
                         * thus, we will subtract 1 (one quotation mark is removed already
                         * by calculating with the indexes only).
                         */
                        return i - CurrentIndex + deltaBytes - 1;
                    case JsonSymbols.Backslash:
                        isInEscapeSequence = true;
                        continue;
                }
            }

            throw CreateEndOfJsonStringNotFoundException();
        }

        private int DetermineByteLengthDeltaOfEscapedCharacter(in ReadOnlySpan<char> json, char secondByteInEscapeSequence, ref int currentIndex)
        {
            switch (secondByteInEscapeSequence)
            {
                case JsonSymbols.QuotationMark:
                case JsonSymbols.Backslash:
                case JsonSymbols.Slash:
                case 'b':
                case 'f':
                case 'n':
                case 'r':
                case 't':
                    ++currentIndex;
                    return 1;
                case 'u':
                    currentIndex += 4;
                    if (currentIndex >= json.Length)
                        throw CreateInvalidEscapeSequenceInJsonStringException();
                    int value = ConvertHexadecimalJsonCharacterToInt32(json[currentIndex - 3]) * 4096; // 16 * 16 * 16 = 4096
                    value += ConvertHexadecimalJsonCharacterToInt32(json[currentIndex - 2]) * 256; // 16 * 16 = 256
                    value += ConvertHexadecimalJsonCharacterToInt32(json[currentIndex - 1]) * 16;
                    value += ConvertHexadecimalJsonCharacterToInt32(json[currentIndex++]);
                    return value < char.MaxValue ? 5 : 4;
            }

            throw CreateInvalidEscapeSequenceInJsonStringException();
        }

        private int ConvertHexadecimalJsonCharacterToInt32(char character)
        {
            switch (character)
            {
                case '0': return 0;
                case '1': return 1;
                case '2': return 2;
                case '3': return 3;
                case '4': return 4;
                case '5': return 5;
                case '6': return 6;
                case '7': return 7;
                case '8': return 8;
                case '9': return 9;
                case 'a':
                case 'A': return 10;
                case 'b':
                case 'B': return 11;
                case 'c':
                case 'C': return 12;
                case 'd':
                case 'D': return 13;
                case 'e':
                case 'E': return 14;
                case 'f':
                case 'F': return 15;
            }

            throw CreateInvalidEscapeSequenceInJsonStringException();
        }

        private SerializationException CreateEndOfJsonStringNotFoundException() =>
            new SerializationException($"Could not find end of JSON string \"{GetErroneousToken()}\" at line {CurrentLine} position {CurrentPosition}.");

        private SerializationException CreateInvalidEscapeSequenceInJsonStringException() =>
            new SerializationException($"Found invalid escape sequence in JSON string \"{GetErroneousToken()}\" at line {CurrentLine} position {CurrentPosition}.");
    }
}