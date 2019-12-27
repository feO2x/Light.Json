using System;
using Light.Json.FrameworkExtensions;

namespace Light.Json.PrimitiveParsing
{
    public static class IntegerParser
    {
        private const long Int64MaximumThreshold = long.MaxValue / 10;
        private const long Int64MinimumThreshold = long.MinValue / 10;

        public static IntegerParseResult TryParseInt64(this ReadOnlySpan<byte> utf8Source, out long number, out int bytesConsumed)
        {
            if (utf8Source.Length == 0)
            {
                bytesConsumed = 0;
                number = default;
                return IntegerParseResult.NoNumber;
            }

            bytesConsumed = 0;
            var currentCharacter = utf8Source[bytesConsumed];

            // Check for sign
            var isPositiveNumber = true;
            if (currentCharacter == '-')
            {
                isPositiveNumber = false;
                if (++bytesConsumed == utf8Source.Length)
                {
                    number = default;
                    return IntegerParseResult.NoNumber;
                }

                currentCharacter = utf8Source[bytesConsumed];
            }
            else if (currentCharacter == '+')
            {
                if (++bytesConsumed == utf8Source.Length)
                {
                    number = default;
                    return IntegerParseResult.NoNumber;
                }

                currentCharacter = utf8Source[bytesConsumed];
            }

            // If the first character is no digit, then it's not a valid number
            if (!currentCharacter.IsJsonDigit())
            {
                number = default;
                return IntegerParseResult.NoNumber;
            }

            // Ignore zeroes at the beginning of the number
            if (currentCharacter == '0')
            {
                IgnoreZero:
                if (++bytesConsumed == utf8Source.Length)
                {
                    number = 0;
                    return IntegerParseResult.ParsingSuccessful;
                }

                currentCharacter = utf8Source[bytesConsumed];
                if (currentCharacter == '0')
                    goto IgnoreZero;

                if (!currentCharacter.IsJsonDigitButNotZero())
                {
                    number = 0;
                    return IntegerParseResult.ParsingSuccessful;
                }
            }

            number = currentCharacter - '0';

            return isPositiveNumber ? TryReadPositiveInt64(utf8Source, ref number, ref bytesConsumed) : TryReadNegativeInt64(utf8Source, ref number, ref bytesConsumed);
        }

        private static IntegerParseResult TryReadPositiveInt64(in ReadOnlySpan<byte> utf8Source, ref long number, ref int bytesConsumed)
        {
            // The calling method already has read the first digit.
            // The digits up to digit 18 can be applied without causing an overflow for Int64.
            var targetIndex = bytesConsumed + 18;
            byte currentCharacter;
            if (utf8Source.Length <= targetIndex)
            {
                for (++bytesConsumed; bytesConsumed < utf8Source.Length; ++bytesConsumed)
                {
                    currentCharacter = utf8Source[bytesConsumed];
                    if (currentCharacter.IsJsonDigit())
                        number = number * 10 + currentCharacter - '0';
                    else if (currentCharacter == '.')
                        return ReadZeroesAfterDecimalDigit(utf8Source, ref number, ref bytesConsumed);
                    else
                        return IntegerParseResult.ParsingSuccessful;
                }

                return IntegerParseResult.ParsingSuccessful;
            }

            for (++bytesConsumed; bytesConsumed < targetIndex; ++bytesConsumed)
            {
                currentCharacter = utf8Source[bytesConsumed];
                if (currentCharacter.IsJsonDigit())
                    number = number * 10 + currentCharacter - '0';
                else if (currentCharacter == '.')
                    return ReadZeroesAfterDecimalDigit(utf8Source, ref number, ref bytesConsumed);
                else
                    return IntegerParseResult.ParsingSuccessful;
            }

            /* 19th digit is tricky:
             *  - long.MaxValue is 9,223,372,036,854,775,807
             *  - it definitely produces an overflow if number is greater than long.MaxValue / 10
             *  - it produces an overflow if number is equal to long.MaxValue / 10 and the last digit is greater than 7 (last digit of long.MaxValue)
             */
            if (bytesConsumed == utf8Source.Length)
                return IntegerParseResult.ParsingSuccessful;

            currentCharacter = utf8Source[bytesConsumed];
            if (!currentCharacter.IsJsonDigit())
                return currentCharacter == '.' ? ReadZeroesAfterDecimalDigit(utf8Source, ref number, ref bytesConsumed) : IntegerParseResult.ParsingSuccessful;

            if (number > Int64MaximumThreshold)
                return IntegerParseResult.Overflow;

            var lastDigit = currentCharacter - '0';
            if (number == Int64MaximumThreshold && lastDigit > 7)
                return IntegerParseResult.Overflow;
            number = number * 10 + lastDigit;

            // 20th digit produces an overflow, no matter what
            if (++bytesConsumed == utf8Source.Length)
                return IntegerParseResult.ParsingSuccessful;

            currentCharacter = utf8Source[bytesConsumed];
            if (currentCharacter.IsJsonDigit())
                return IntegerParseResult.Overflow;
            if (currentCharacter == '.')
                return ReadZeroesAfterDecimalDigit(utf8Source, ref number, ref bytesConsumed);
            return IntegerParseResult.ParsingSuccessful;
        }

        private static IntegerParseResult TryReadNegativeInt64(in ReadOnlySpan<byte> utf8Source, ref long number, ref int bytesConsumed)
        {
            // The calling method already has read the first digit.
            // The digits up to digit 18 can be applied without causing an overflow for Int64.
            number = -number;
            var targetIndex = bytesConsumed + 18;
            byte currentCharacter;
            if (utf8Source.Length <= targetIndex)
            {
                for (++bytesConsumed; bytesConsumed < utf8Source.Length; ++bytesConsumed)
                {
                    currentCharacter = utf8Source[bytesConsumed];
                    if (currentCharacter.IsJsonDigit())
                        number = number * 10 - (currentCharacter - '0');
                    else if (currentCharacter == '.')
                        return ReadZeroesAfterDecimalDigit(utf8Source, ref number, ref bytesConsumed);
                    else
                        return IntegerParseResult.ParsingSuccessful;
                }

                return IntegerParseResult.ParsingSuccessful;
            }

            for (++bytesConsumed; bytesConsumed < targetIndex; ++bytesConsumed)
            {
                currentCharacter = utf8Source[bytesConsumed];
                if (currentCharacter.IsJsonDigit())
                    number = number * 10 - (currentCharacter - '0');
                else if (currentCharacter == '.')
                    return ReadZeroesAfterDecimalDigit(utf8Source, ref number, ref bytesConsumed);
                else
                    return IntegerParseResult.ParsingSuccessful;
            }

            /* 19th digit is tricky:
             *  - long.MinValue is -9,223,372,036,854,775,808
             *  - it definitely produces an overflow if number is less than long.MinValue / 10
             *  - it produces an overflow if number is equal to long.MinValue / 10 and the last digit is greater than 7 (last digit of long.MaxValue)
             */
            if (bytesConsumed == utf8Source.Length)
                return IntegerParseResult.ParsingSuccessful;

            currentCharacter = utf8Source[bytesConsumed];
            if (!currentCharacter.IsJsonDigit())
                return currentCharacter == '.' ? ReadZeroesAfterDecimalDigit(utf8Source, ref number, ref bytesConsumed) : IntegerParseResult.ParsingSuccessful;

            if (number < Int64MinimumThreshold)
                return IntegerParseResult.Overflow;

            var lastDigit = currentCharacter - '0';
            if (number == Int64MinimumThreshold && lastDigit > 8)
                return IntegerParseResult.Overflow;
            number = number * 10 - lastDigit;

            // 20th digit produces an overflow, no matter what
            if (++bytesConsumed == utf8Source.Length)
                return IntegerParseResult.ParsingSuccessful;

            currentCharacter = utf8Source[bytesConsumed];
            if (currentCharacter.IsJsonDigit())
                return IntegerParseResult.Overflow;
            if (currentCharacter == '.')
                return ReadZeroesAfterDecimalDigit(utf8Source, ref number, ref bytesConsumed);
            return IntegerParseResult.ParsingSuccessful;
        }

        public static IntegerParseResult TryParseInt64(this ReadOnlySpan<char> source, out long number, out int bytesConsumed)
        {
            if (source.Length == 0)
            {
                bytesConsumed = 0;
                number = default;
                return IntegerParseResult.NoNumber;
            }

            bytesConsumed = 0;
            var currentCharacter = source[bytesConsumed];

            // Check for sign
            var isPositiveNumber = true;
            if (currentCharacter == '-')
            {
                isPositiveNumber = false;
                if (++bytesConsumed == source.Length)
                {
                    number = default;
                    return IntegerParseResult.NoNumber;
                }

                currentCharacter = source[bytesConsumed];
            }
            else if (currentCharacter == '+')
            {
                if (++bytesConsumed == source.Length)
                {
                    number = default;
                    return IntegerParseResult.NoNumber;
                }

                currentCharacter = source[bytesConsumed];
            }

            // If the first character is no digit, then it's not a valid number
            if (!currentCharacter.IsJsonDigit())
            {
                number = default;
                return IntegerParseResult.NoNumber;
            }

            // Ignore zeroes at the beginning of the number
            if (currentCharacter == '0')
            {
                IgnoreZero:
                if (++bytesConsumed == source.Length)
                {
                    number = 0;
                    return IntegerParseResult.ParsingSuccessful;
                }

                currentCharacter = source[bytesConsumed];
                if (currentCharacter == '0')
                    goto IgnoreZero;

                if (!currentCharacter.IsJsonDigitButNotZero())
                {
                    number = 0;
                    return IntegerParseResult.ParsingSuccessful;
                }
            }

            number = currentCharacter - '0';

            return isPositiveNumber ? TryReadPositiveInt64(source, ref number, ref bytesConsumed) : TryReadNegativeInt64(source, ref number, ref bytesConsumed);
        }

        private static IntegerParseResult TryReadPositiveInt64(in ReadOnlySpan<char> source, ref long number, ref int bytesConsumed)
        {
            // The calling method already has read the first digit.
            // The digits up to digit 18 can be applied without causing an overflow for Int64.
            var targetIndex = bytesConsumed + 18;
            char currentCharacter;
            if (source.Length <= targetIndex)
            {
                for (++bytesConsumed; bytesConsumed < source.Length; ++bytesConsumed)
                {
                    currentCharacter = source[bytesConsumed];
                    if (currentCharacter.IsJsonDigit())
                        number = number * 10 + currentCharacter - '0';
                    else if (currentCharacter == '.')
                        return ReadZeroesAfterDecimalDigit(source, ref number, ref bytesConsumed);
                    else
                        return IntegerParseResult.ParsingSuccessful;
                }

                return IntegerParseResult.ParsingSuccessful;
            }

            for (++bytesConsumed; bytesConsumed < targetIndex; ++bytesConsumed)
            {
                currentCharacter = source[bytesConsumed];
                if (currentCharacter.IsJsonDigit())
                    number = number * 10 + currentCharacter - '0';
                else if (currentCharacter == '.')
                    return ReadZeroesAfterDecimalDigit(source, ref number, ref bytesConsumed);
                else
                    return IntegerParseResult.ParsingSuccessful;
            }

            /* 19th digit is tricky:
             *  - long.MaxValue is 9,223,372,036,854,775,807
             *  - it definitely produces an overflow if number is greater than long.MaxValue / 10
             *  - it produces an overflow if number is equal to long.MaxValue / 10 and the last digit is greater than 7 (last digit of long.MaxValue)
             */
            if (bytesConsumed == source.Length)
                return IntegerParseResult.ParsingSuccessful;

            currentCharacter = source[bytesConsumed];
            if (!currentCharacter.IsJsonDigit())
                return currentCharacter == '.' ? ReadZeroesAfterDecimalDigit(source, ref number, ref bytesConsumed) : IntegerParseResult.ParsingSuccessful;

            if (number > Int64MaximumThreshold)
                return IntegerParseResult.Overflow;

            var lastDigit = currentCharacter - '0';
            if (number == Int64MaximumThreshold && lastDigit > 7)
                return IntegerParseResult.Overflow;
            number = number * 10 + lastDigit;

            // 20th digit produces an overflow, no matter what
            if (++bytesConsumed == source.Length)
                return IntegerParseResult.ParsingSuccessful;

            currentCharacter = source[bytesConsumed];
            if (currentCharacter == '.')
                return ReadZeroesAfterDecimalDigit(source, ref number, ref bytesConsumed);

            return currentCharacter.IsJsonDigit() ? IntegerParseResult.Overflow : IntegerParseResult.ParsingSuccessful;
        }

        private static IntegerParseResult TryReadNegativeInt64(in ReadOnlySpan<char> source, ref long number, ref int bytesConsumed)
        {
            // The calling method already has read the first digit.
            // The digits up to digit 18 can be applied without causing an overflow for Int64.
            number = -number;
            var targetIndex = bytesConsumed + 18;
            char currentCharacter;
            if (source.Length <= targetIndex)
            {
                for (++bytesConsumed; bytesConsumed < source.Length; ++bytesConsumed)
                {
                    currentCharacter = source[bytesConsumed];
                    if (currentCharacter.IsJsonDigit())
                        number = number * 10 - (currentCharacter - '0');
                    else if (currentCharacter == '.')
                        return ReadZeroesAfterDecimalDigit(source, ref number, ref bytesConsumed);
                    else
                        return IntegerParseResult.ParsingSuccessful;
                }

                return IntegerParseResult.ParsingSuccessful;
            }

            for (++bytesConsumed; bytesConsumed < targetIndex; ++bytesConsumed)
            {
                currentCharacter = source[bytesConsumed];
                if (currentCharacter.IsJsonDigit())
                    number = number * 10 - (currentCharacter - '0');
                else if (currentCharacter == '.')
                        return ReadZeroesAfterDecimalDigit(source, ref number, ref bytesConsumed);
                else
                    return IntegerParseResult.ParsingSuccessful;
            }

            /* 19th digit is tricky:
             *  - long.MinValue is -9,223,372,036,854,775,808
             *  - it definitely produces an overflow if number is less than long.MinValue / 10
             *  - it produces an overflow if number is equal to long.MinValue / 10 and the last digit is greater than 7 (last digit of long.MaxValue)
             */
            if (bytesConsumed == source.Length)
                return IntegerParseResult.ParsingSuccessful;

            currentCharacter = source[bytesConsumed];
            if (!currentCharacter.IsJsonDigit())
                return currentCharacter == '.' ? ReadZeroesAfterDecimalDigit(source, ref number, ref bytesConsumed) : IntegerParseResult.ParsingSuccessful;

            if (number < Int64MinimumThreshold)
                return IntegerParseResult.Overflow;

            var lastDigit = currentCharacter - '0';
            if (number == Int64MinimumThreshold && lastDigit > 8)
                return IntegerParseResult.Overflow;
            number = number * 10 - lastDigit;

            // 20th digit produces an overflow, no matter what
            if (++bytesConsumed == source.Length)
                return IntegerParseResult.ParsingSuccessful;

            currentCharacter = source[bytesConsumed];
            if (currentCharacter.IsJsonDigit())
                return IntegerParseResult.Overflow;
            if (currentCharacter == '.')
                return ReadZeroesAfterDecimalDigit(source, ref number, ref bytesConsumed);
            return IntegerParseResult.ParsingSuccessful;
        }

        public static IntegerParseResult TryParseUInt64(this ReadOnlySpan<byte> utf8Source, out ulong number, out int bytesConsumed)
        {
            if (utf8Source.Length == 0)
            {
                bytesConsumed = 0;
                number = default;
                return IntegerParseResult.NoNumber;
            }

            bytesConsumed = 0;
            var currentCharacter = utf8Source[bytesConsumed];

            // Check for sign
            if (currentCharacter == '-')
            {
                number = 0;
                return TryReadNegativeZero(utf8Source, ref bytesConsumed);
            }

            if (currentCharacter == '+')
            {
                if (++bytesConsumed == utf8Source.Length)
                {
                    number = default;
                    return IntegerParseResult.NoNumber;
                }

                currentCharacter = utf8Source[bytesConsumed];
            }

            throw new NotImplementedException();
        }

        private static IntegerParseResult TryReadNegativeZero(in ReadOnlySpan<byte> utf8Source, ref int bytesConsumed)
        {
            // This method is called when the first character of the number is a digit
            if (++bytesConsumed == utf8Source.Length)
                return IntegerParseResult.NoNumber;

            var currentCharacter = utf8Source[bytesConsumed];
            if (currentCharacter != '0')
                return IntegerParseResult.NoNumber;

            throw new NotImplementedException();
        }

        private static IntegerParseResult ReadZeroesAfterDecimalDigit(in ReadOnlySpan<byte> utf8Source, ref long number, ref int bytesConsumed)
        {
            // When this method is called, the decimal point (.) was already parsed
            if (++bytesConsumed == utf8Source.Length)
                return IntegerParseResult.NoNumber;

            IgnoreZeroes:
            var currentCharacter = utf8Source[bytesConsumed];
            if (currentCharacter == '0')
            {
                if (++bytesConsumed == utf8Source.Length)
                    return IntegerParseResult.ParsingSuccessful;

                goto IgnoreZeroes;
            }

            return currentCharacter.IsJsonDigitButNotZero() ? IntegerParseResult.NonZeroDigitsAfterDecimalPoint : IntegerParseResult.ParsingSuccessful;
        }

        private static IntegerParseResult ReadZeroesAfterDecimalDigit(in ReadOnlySpan<char> source, ref long number, ref int bytesConsumed)
        {
            // When this method is called, the decimal point (.) was already parsed
            if (++bytesConsumed == source.Length)
                return IntegerParseResult.NoNumber;

            IgnoreZeroes:
            var currentCharacter = source[bytesConsumed];
            if (currentCharacter == '0')
            {
                if (++bytesConsumed == source.Length)
                    return IntegerParseResult.ParsingSuccessful;

                goto IgnoreZeroes;
            }

            return currentCharacter.IsJsonDigitButNotZero() ? IntegerParseResult.NonZeroDigitsAfterDecimalPoint : IntegerParseResult.ParsingSuccessful;
        }
    }
}