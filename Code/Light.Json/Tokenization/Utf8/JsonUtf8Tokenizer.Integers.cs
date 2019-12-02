using System;
using System.Buffers.Text;

namespace Light.Json.Tokenization.Utf8
{
    public partial struct JsonUtf8Tokenizer
    {
        public int ReadInt32()
        {
            return (int) ReadSignedInteger(int.MinValue, int.MaxValue, "System.Int32");
        }

        private long ReadSignedInteger(long minimum, long maximum, string typeName)
        {
            var json = _json.Span;
            if (!TrySkipWhiteSpace(json))
                throw new DeserializationException("Expected JSON integer number but found end of document.");

            json = json.Slice(_currentIndex);
            if (!Utf8Parser.TryParse(json, out long parsedNumber, out var currentIndex))
                throw CreateInvalidNumberException();

            if (json.Length == currentIndex)
                goto UpdateStateAndReturnParsedNumber;

            var currentCharacter = json[currentIndex++];
            switch (currentCharacter)
            {
                case (byte) '.':
                    parsedNumber = EnsureDigitsAfterDecimalPointAreZero(json, ref currentIndex, parsedNumber);
                    goto UpdateStateAndReturnParsedNumber;
                case (byte) 'e':
                case (byte) 'E':
                    parsedNumber = ParseAndApplyExponent(json, ref currentIndex, parsedNumber);
                    goto UpdateStateAndReturnParsedNumber;
            }

            UpdateStateAndReturnParsedNumber:
            if (parsedNumber < minimum || parsedNumber > maximum)
                throw CreateOverflowException(typeName);

            _currentPosition += currentIndex;
            _currentIndex += currentIndex;
            return parsedNumber;
        }

        private long EnsureDigitsAfterDecimalPointAreZero(ReadOnlySpan<byte> json, ref int externalIndex, long parsedNumber)
        {
            if (externalIndex == json.Length)
                throw CreateInvalidNumberException();

            json = json.Slice(externalIndex);
            if (!Utf8Parser.TryParse(json, out long @decimal, out var internalIndex))
                throw CreateInvalidNumberException();

            if (@decimal != 0L)
                throw CreateInvalidDecimalInIntegerNumberException();

            if (internalIndex == json.Length)
                goto ReturnParsedNumber;

            var currentCharacter = json[internalIndex];
            if (currentCharacter == 'e' || currentCharacter == 'E')
                parsedNumber = ParseAndApplyExponent(json, ref internalIndex, parsedNumber);

            ReturnParsedNumber:
            externalIndex += internalIndex;
            return parsedNumber;
        }

        private long ParseAndApplyExponent(ReadOnlySpan<byte> json, ref int externalIndex, long parsedNumber)
        {
            if (externalIndex == json.Length)
                throw CreateInvalidNumberException();

            json = json.Slice(externalIndex);
            if (!Utf8Parser.TryParse(json, out int exponent, out var internalIndex))
                throw CreateInvalidNumberException();

            if (exponent != 0)
                parsedNumber = exponent > 0 ? ApplyPositiveExponent(parsedNumber, exponent) : ApplyNegativeExponent(parsedNumber, exponent);

            externalIndex += internalIndex;
            return parsedNumber;
        }

        private long ApplyPositiveExponent(long parsedNumber, int exponent)
        {
            while (exponent-- > 0)
            {
                if (parsedNumber < 100_000_000_000_000_000L)
                {
                    parsedNumber *= 10L;
                }
                else
                {
                    try
                    {
                        checked
                        {
                            parsedNumber *= 10;
                        }
                    }
                    catch (OverflowException overflowException)
                    {
                        throw CreateInt64OverflowException(overflowException);
                    }
                }
            }

            return parsedNumber;
        }

        private long ApplyNegativeExponent(long parsedNumber, int exponent)
        {
            while (exponent-- > 0)
            {
                parsedNumber = Math.DivRem(parsedNumber, 10, out var remainder);
                if (remainder > 0)
                    throw CreateInvalidNegativeExponentException();
            }

            return parsedNumber;
        }

        private DeserializationException CreateInvalidNumberException() =>
            new DeserializationException($"Found invalid JSON number {GetErroneousToken()} at line {_currentLine} position {_currentPosition}.");

        private DeserializationException CreateInvalidDecimalInIntegerNumberException() =>
            new DeserializationException($"The JSON number {GetErroneousToken()} at line {_currentLine} position {_currentPosition} cannot be parsed to an integer number.");

        private DeserializationException CreateInt64OverflowException(Exception innerException) =>
            new DeserializationException($"The JSON number {GetErroneousToken()} at line {_currentLine} position {_currentPosition} causes an overflow.", innerException);

        private DeserializationException CreateOverflowException(string typeName) =>
            new DeserializationException($"The JSON number {GetErroneousToken()} at line {_currentLine} position {_currentPosition} cannot be parsed to type \"{typeName}\" without producing an overflow.");

        private DeserializationException CreateInvalidNegativeExponentException() =>
            new DeserializationException($"The negative exponent of JSON number {GetErroneousToken()} at line {_currentLine} position {_currentPosition} cannot be parsed to an integer number.");
    }
}