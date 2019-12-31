﻿using Light.Json.PrimitiveParsing;

namespace Light.Json.Tokenization.Utf8
{
    public partial struct JsonUtf8Tokenizer
    {
        public int ReadInt32()
        {
            var json = _json.Span;
            if (!TrySkipWhiteSpace(json))
                throw new DeserializationException("Expected JSON integer number but found end of document.");
            json = json.Slice(_currentIndex);
            var result = json.TryParseInt32(out var value, out var bytesConsumed);
            switch (result)
            {
                case IntegerParseResult.ParsingSuccessful:
                    _currentIndex += bytesConsumed;
                    _currentPosition += bytesConsumed;
                    return value;
                case IntegerParseResult.NoNumber:
                    throw new DeserializationException($"Expected JSON integer number but token \"{GetErroneousToken()}\" at line {_currentLine} position {_currentPosition} does not represent a number.");
                default:
                    throw new DeserializationException($"The token \"{GetErroneousToken()}\" at line {_currentLine} position {_currentPosition} produces an overflow when parsing it to System.Int32.");
            }
        }
    }
}