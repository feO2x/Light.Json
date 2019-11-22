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

            if (currentCharacter == '0')
            {
                _currentPosition += ++i - _currentIndex;
                _currentIndex = i;
                return 0;
            }

            if (!currentCharacter.IsJsonDigitButNotZero())
                throw CreateInvalidNumberException();

            ++i;
            var parsedNumber = isNumberPositive ? 
                                  ReadPositiveInteger(json, currentCharacter, ref i, maximumValue, maximumNumberOfDigits) : 
                                  ReadNegativeInteger(json, currentCharacter, ref i, minimumValue, maximumNumberOfDigits);

            _currentPosition += i - _currentIndex;
            _currentIndex = i;

            return parsedNumber;
        }

        private long ReadPositiveInteger(in ReadOnlySpan<char> json, char currentCharacter, ref int currentIndex, long maximumValue, int maximumNumberOfDigits)
        {
            var parsedNumber = (long) (currentCharacter - 48);
            var numberOfDigits = 1;

            for (; currentIndex < json.Length; ++currentIndex)
            {
                currentCharacter = json[currentIndex];
                if (currentCharacter.IsJsonDigit())
                {
                    parsedNumber = parsedNumber * 10 + (currentCharacter - 48);
                    if (++numberOfDigits >= maximumNumberOfDigits && parsedNumber > maximumValue)
                        throw new DeserializationException($"The JSON number {GetErroneousToken()} is too big.");
                }
                else if (currentCharacter == '.')
                {
                    ++currentIndex;
                    CheckIfOnlyZeroesAreAfterDecimalPoint(json, ref currentIndex);
                    break;
                }
            }

            return parsedNumber;
        }

        private long ReadNegativeInteger(in ReadOnlySpan<char> json, char currentCharacter, ref int currentIndex, long minimumValue, int maximumNumberOfDigits)
        {
            var parsedNumber = -(long) (currentCharacter - 48);
            var numberOfDigits = 1;

            for (; currentIndex < json.Length; ++currentIndex)
            {
                currentCharacter = json[currentIndex];
                if (currentCharacter.IsJsonDigit())
                {
                    parsedNumber = parsedNumber * 10 - (currentCharacter - 48);
                    if (++numberOfDigits >= maximumNumberOfDigits && parsedNumber < minimumValue)
                        throw new DeserializationException($"The JSON number {GetErroneousToken()} is too small.");
                }
                else if (currentCharacter == '.')
                {
                    ++currentIndex;
                    CheckIfOnlyZeroesAreAfterDecimalPoint(json, ref currentIndex);
                    break;
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

        private DeserializationException CreateInvalidNumberException() =>
            new DeserializationException($"Found invalid JSON number {GetErroneousToken()} at line {_currentLine} position {_currentPosition}.");

        private DeserializationException CreateInvalidDecimalInIntegerNumberException() =>
            new DeserializationException($"The JSON number {GetErroneousToken()} at line {_currentLine} position {_currentPosition} cannot be parsed to an integer number");

        private DeserializationException CreateNumberMustNotStartWithZeroException() =>
            new DeserializationException($"Found invalid JSON number {GetErroneousToken()} at line {_currentLine} position {_currentPosition} (numbers must not start with leading zeroes).");

        private DeserializationException CreateInvalidExponentException() =>
            new DeserializationException($"Found invalid JSON number {GetErroneousToken()} at line {_currentLine} position {_currentPosition} (exponent is invalid).");
    }
}