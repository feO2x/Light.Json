﻿using System;
using Light.Json.FrameworkExtensions;

namespace Light.Json.Tokenization.Utf8
{
    public partial struct JsonUtf8Tokenizer
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

        private long ReadPositiveInteger(in ReadOnlySpan<byte> json, byte currentCharacter, ref int currentIndex, long maximumValue, int maximumNumberOfDigits)
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

            return parsedNumber;
        }

        private long ReadNegativeInteger(in ReadOnlySpan<byte> json, byte currentCharacter, ref int currentIndex, long minimumValue, int maximumNumberOfDigits)
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

        private void CheckIfOnlyZeroesAreAfterDecimalPoint(in ReadOnlySpan<byte> json, ref int i)
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
            new DeserializationException($"The JSON number {GetErroneousToken()} at line {_currentLine} position {_currentPosition} cannot be parsed to an integer number.");
    }
}