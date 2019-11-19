using System;
using Light.Json.FrameworkExtensions;

namespace Light.Json.Tokenization.Utf16
{
    public partial struct JsonUtf16Tokenizer
    {
        public int ReadInt32()
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

            // int.MaxValue is 2147483647. It has a maximum of 10 digits
            var parsedNumber = currentCharacter - 48; // 0 is the 48th character in ASCII
            if (!isNumberPositive)
                parsedNumber = -parsedNumber;
            var numberOfDigits = 1;
            for (++i; i < json.Length; ++i)
            {
                currentCharacter = json[i];
                if (currentCharacter.IsJsonDigit())
                {
                    if (numberOfDigits++ < 9)
                    {
                        if (isNumberPositive)
                        {
                            unchecked
                            {
                                parsedNumber = parsedNumber * 10 + (currentCharacter - 48);
                            }
                        }
                        else
                        {
                            unchecked
                            {
                                parsedNumber = parsedNumber * 10 - (currentCharacter - 48);
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            if (isNumberPositive)
                            {
                                checked
                                {
                                    parsedNumber = parsedNumber * 10 + (currentCharacter - 48);
                                }
                            }
                            else
                            {
                                checked
                                {
                                    parsedNumber = parsedNumber * 10 - (currentCharacter - 48);
                                }
                            }
                        }
                        catch (OverflowException exception)
                        {
                            throw new DeserializationException($"The JSON number {GetErroneousToken()} is too big", exception);
                        }
                    }
                    
                    continue;
                }

                if (currentCharacter == '.')
                {
                    CheckIfOnlyZeroesAreAfterDecimalPoint(json, i + 1);
                    break;
                }
            }

            return parsedNumber;
        }

        private void CheckIfOnlyZeroesAreAfterDecimalPoint(in ReadOnlySpan<char> json, int i)
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