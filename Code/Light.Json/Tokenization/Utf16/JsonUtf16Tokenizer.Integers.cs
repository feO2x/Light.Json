using System;
using Light.Json.FrameworkExtensions;

namespace Light.Json.Tokenization.Utf16
{
    public partial struct JsonUtf16Tokenizer
    {
        public int ReadInt32()
        {
            return (int) ReadSignedInteger(int.MinValue, int.MaxValue, 10);
        }

        private long ReadSignedInteger(long minimumValue, long maximumValue, int maximumNumberOfDigits)
        {
            var json = _json.Span;
            if (!TrySkipWhiteSpace(json))
                throw new DeserializationException("Expected JSON integer number but found end of document.");

            var i = _currentIndex;
            var currentCharacter = json[i];
            var isNumberPositive = true;
            if (currentCharacter == '-')
            {
                isNumberPositive = false;
                if (++i == json.Length)
                    throw CreateInvalidNumberException();
                currentCharacter = json[i];
            }

            if (!currentCharacter.IsJsonDigit())
                throw CreateInvalidNumberException();

            ++i;
            var parsedNumber = isNumberPositive ? ReadPositiveInteger(json, currentCharacter, ref i, maximumValue, maximumNumberOfDigits) : ReadNegativeInteger(json, currentCharacter, ref i, minimumValue, maximumNumberOfDigits);

            _currentPosition += i - _currentIndex;
            _currentIndex = i;

            return parsedNumber;
        }

        private long ReadPositiveInteger(in ReadOnlySpan<char> json, char currentCharacter, ref int currentIndex, long maximumValue, int maximumNumberOfDigits)
        {
            var parsedNumber = (long) (currentCharacter - '0');
            var numberOfDigits = 1;

            for (; currentIndex < json.Length; ++currentIndex)
            {
                currentCharacter = json[currentIndex];
                if (!currentCharacter.IsJsonDigit())
                    break;

                parsedNumber = parsedNumber * 10 + (currentCharacter - '0');
                if (++numberOfDigits >= maximumNumberOfDigits && parsedNumber > maximumValue)
                    throw new DeserializationException($"The JSON number {GetErroneousToken()} is too big.");
            }

            if (currentCharacter == '.')
            {
                ++currentIndex;
                CheckIfOnlyZeroesAreAfterDecimalPoint(json, ref currentIndex);
            }
            else if (currentCharacter == 'e' || currentCharacter == 'E')
            {
                ++currentIndex;
                var exponent = ParseExponent(json, ref currentIndex);
                parsedNumber = ApplyExponent(parsedNumber, exponent);
            }

            return parsedNumber;
        }

        private long ReadNegativeInteger(in ReadOnlySpan<char> json, char currentCharacter, ref int currentIndex, long minimumValue, int maximumNumberOfDigits)
        {
            var parsedNumber = -(long) (currentCharacter - '0');
            var numberOfDigits = 1;

            for (; currentIndex < json.Length; ++currentIndex)
            {
                currentCharacter = json[currentIndex];
                if (!currentCharacter.IsJsonDigit())
                    break;

                parsedNumber = parsedNumber * 10 - (currentCharacter - '0');
                if (++numberOfDigits >= maximumNumberOfDigits && parsedNumber < minimumValue)
                    throw new DeserializationException($"The JSON number {GetErroneousToken()} is too small.");
            }

            if (currentCharacter == '.')
            {
                ++currentIndex;
                CheckIfOnlyZeroesAreAfterDecimalPoint(json, ref currentIndex);
            }


            return parsedNumber;
        }

        private int ParseExponent(in ReadOnlySpan<char> json, ref int currentIndex)
        {
            var isMantissaPositive = true;
            var currentCharacter = json[currentIndex];
            if (currentCharacter == '-')
            {
                isMantissaPositive = false;
                if (++currentIndex == json.Length)
                    throw CreateInvalidExponentException();
                currentCharacter = json[currentIndex];
            }
            else if (currentCharacter == '+')
            {
                if (++currentIndex == json.Length)
                    throw CreateInvalidExponentException();
                currentCharacter = json[currentIndex];
            }

            if (!currentCharacter.IsJsonDigit())
                throw CreateInvalidExponentException();

            var exponent = currentCharacter - '0';
            var numberOfDigits = 1;
            for (; currentIndex < json.Length; ++currentIndex)
            {
                currentCharacter = json[currentIndex];
                if (currentCharacter.IsJsonDigit())
                    break;

                if (++numberOfDigits < 10)
                {
                    exponent = exponent * 10 + (currentCharacter - '0');
                }
                else
                {

                    try
                    {
                        checked
                        {
                            exponent = exponent * 10 + (currentCharacter - '0');
                        }
                    }
                    catch (OverflowException exception)
                    {
                        throw CreateExponentOverflowException(exception);
                    }
                }
            }

            return isMantissaPositive ? exponent : -exponent;
        }

        private long ApplyExponent(long parsedNumber, int exponent)
        {
            if (exponent > 0)
            {
                while (exponent-- > 0)
                {
                    if (parsedNumber < 100_000_000)
                    {
                        parsedNumber *= 10;
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
                        catch (OverflowException exception)
                        {
                            throw CreateOverflowException(exception);
                        }
                    }
                }
            }

            return parsedNumber;
        }

        private void CheckIfOnlyZeroesAreAfterDecimalPoint(in ReadOnlySpan<char> json, ref int i)
        {
            if (i == json.Length)
                throw CreateInvalidNumberException();

            var digitAfterDecimalPoint = json[i];
            if (digitAfterDecimalPoint != '0')
                throw CreateInvalidNumberException();

            for (++i; i < json.Length; ++i)
            {
                var currentCharacter = json[i];
                if (currentCharacter == '0')
                    continue;
                if (currentCharacter.IsJsonDigitButNotZero())
                    throw CreateInvalidDecimalInIntegerNumberException();

                return;
            }
        }

        private DeserializationException CreateInvalidNumberException()
        {
            return new DeserializationException($"Found invalid JSON number {GetErroneousToken()} at line {_currentLine} position {_currentPosition}.");
        }

        private DeserializationException CreateInvalidDecimalInIntegerNumberException()
        {
            return new DeserializationException($"The JSON number {GetErroneousToken()} at line {_currentLine} position {_currentPosition} cannot be parsed to an integer number.");
        }

        private DeserializationException CreateInvalidExponentException()
        {
            return new DeserializationException($"Found invalid JSON number {GetErroneousToken()} at line {_currentLine} position {_currentPosition} (exponent is invalid).");
        }

        private DeserializationException CreateExponentOverflowException(Exception innerException = null)
        {
            return new DeserializationException($"The JSON number {GetErroneousToken()} at line {_currentLine} position {_currentPosition} cannot be parsed because the exponent overflows.", innerException);
        }

        private DeserializationException CreateOverflowException(OverflowException innerException = null)
        {
            return new DeserializationException($"The JSON number {GetErroneousToken()} at line {_currentLine} position {_currentPosition} cannot be parsed because it produces an overflow.", innerException);
        }
    }
}