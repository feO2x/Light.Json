using System;
using Light.Json.FrameworkExtensions;

namespace Light.Json.PrimitiveParsing
{
    public static class IntegerParser
    {
        private const long Int64MaximumThreshold = long.MaxValue / 10;
        private const long Int64MinimumThreshold = long.MinValue / 10;

        public static IntegerParseResult TryParseInt64(this ReadOnlySpan<byte> source, out long number, out int bytesConsumed)
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
                    return IntegerParseResult.Success;
                }

                currentCharacter = source[bytesConsumed];
                if (currentCharacter == '0')
                    goto IgnoreZero;

                if (!currentCharacter.IsJsonDigitButNotZero())
                {
                    number = 0;
                    return IntegerParseResult.Success;
                }
            }

            number = currentCharacter - '0';

            return isPositiveNumber ? TryReadPositiveInt64(source, ref number, ref bytesConsumed) : TryReadNegativeInt64(source, ref number, ref bytesConsumed);
        }

        private static IntegerParseResult TryReadPositiveInt64(in ReadOnlySpan<byte> source, ref long number, ref int bytesConsumed)
        {
            // The calling method already has read the first digit.
            // The digits up to digit 18 can be applied without causing an overflow for Int64.
            var targetIndex = bytesConsumed + 18;
            byte currentCharacter;
            if (source.Length <= targetIndex)
            {
                for (++bytesConsumed; bytesConsumed < source.Length; ++bytesConsumed)
                {
                    currentCharacter = source[bytesConsumed];
                    if (currentCharacter.IsJsonDigit())
                        number = number * 10 + currentCharacter - '0';
                    else
                        return IntegerParseResult.Success;
                }

                return IntegerParseResult.Success;
            }

            for (++bytesConsumed; bytesConsumed < targetIndex; ++bytesConsumed)
            {
                currentCharacter = source[bytesConsumed];
                if (currentCharacter.IsJsonDigit())
                    number = number * 10 + currentCharacter - '0';
                else
                    return IntegerParseResult.Success;
            }

            /* 19th digit is tricky:
             *  - long.MaxValue is 9,223,372,036,854,775,807
             *  - it definitely produces an overflow if number is greater than long.MaxValue / 10
             *  - it produces an overflow if number is equal to long.MaxValue / 10 and the last digit is greater than 7 (last digit of long.MaxValue)
             */
            if (bytesConsumed == source.Length)
                return IntegerParseResult.Success;

            currentCharacter = source[bytesConsumed];
            if (!currentCharacter.IsJsonDigit())
                return IntegerParseResult.Success;

            if (number > Int64MaximumThreshold)
                return IntegerParseResult.Overflow;

            var lastDigit = currentCharacter - '0';
            if (number == Int64MaximumThreshold && lastDigit > 7)
                return IntegerParseResult.Overflow;
            number = number * 10 + lastDigit;

            // 20th digit produces an overflow, no matter what
            if (++bytesConsumed == source.Length)
                return IntegerParseResult.Success;

            currentCharacter = source[bytesConsumed];
            return !currentCharacter.IsJsonDigit() ? IntegerParseResult.Success : IntegerParseResult.Overflow;
        }

        private static IntegerParseResult TryReadNegativeInt64(in ReadOnlySpan<byte> source, ref long number, ref int bytesConsumed)
        {
            // The calling method already has read the first digit.
            // The digits up to digit 18 can be applied without causing an overflow for Int64.
            number = -number;
            var targetIndex = bytesConsumed + 18;
            byte currentCharacter;
            if (source.Length <= targetIndex)
            {
                for (++bytesConsumed; bytesConsumed < source.Length; ++bytesConsumed)
                {
                    currentCharacter = source[bytesConsumed];
                    if (currentCharacter.IsJsonDigit())
                        number = number * 10 - (currentCharacter - '0');
                    else
                        return IntegerParseResult.Success;
                }

                return IntegerParseResult.Success;
            }

            for (++bytesConsumed; bytesConsumed < targetIndex; ++bytesConsumed)
            {
                currentCharacter = source[bytesConsumed];
                if (currentCharacter.IsJsonDigit())
                    number = number * 10 - (currentCharacter - '0');
                else
                    return IntegerParseResult.Success;
            }

            /* 19th digit is tricky:
             *  - long.MinValue is -9,223,372,036,854,775,808
             *  - it definitely produces an overflow if number is less than long.MinValue / 10
             *  - it produces an overflow if number is equal to long.MinValue / 10 and the last digit is greater than 7 (last digit of long.MaxValue)
             */
             if (bytesConsumed == source.Length)
                return IntegerParseResult.Success;

            currentCharacter = source[bytesConsumed];
            if (!currentCharacter.IsJsonDigit())
                return IntegerParseResult.Success;

            if (number < Int64MinimumThreshold)
                return IntegerParseResult.Overflow;

            var lastDigit = currentCharacter - '0';
            if (number == Int64MinimumThreshold && lastDigit > 8)
                return IntegerParseResult.Overflow;
            number = number * 10 - lastDigit;

            // 20th digit produces an overflow, no matter what
            if (++bytesConsumed == source.Length)
                return IntegerParseResult.Success;

            currentCharacter = source[bytesConsumed];
            return !currentCharacter.IsJsonDigit() ? IntegerParseResult.Success : IntegerParseResult.Overflow;
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
                    return IntegerParseResult.Success;
                }

                currentCharacter = source[bytesConsumed];
                if (currentCharacter == '0')
                    goto IgnoreZero;

                if (!currentCharacter.IsJsonDigitButNotZero())
                {
                    number = 0;
                    return IntegerParseResult.Success;
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
                    else
                        return IntegerParseResult.Success;
                }

                return IntegerParseResult.Success;
            }

            for (++bytesConsumed; bytesConsumed < targetIndex; ++bytesConsumed)
            {
                currentCharacter = source[bytesConsumed];
                if (currentCharacter.IsJsonDigit())
                    number = number * 10 + currentCharacter - '0';
                else
                    return IntegerParseResult.Success;
            }

            /* 19th digit is tricky:
             *  - long.MaxValue is 9,223,372,036,854,775,807
             *  - it definitely produces an overflow if number is greater than long.MaxValue / 10
             *  - it produces an overflow if number is equal to long.MaxValue / 10 and the last digit is greater than 7 (last digit of long.MaxValue)
             */
            if (bytesConsumed == source.Length)
                return IntegerParseResult.Success;

            currentCharacter = source[bytesConsumed];
            if (!currentCharacter.IsJsonDigit())
                return IntegerParseResult.Success;

            if (number > Int64MaximumThreshold)
                return IntegerParseResult.Overflow;

            var lastDigit = currentCharacter - '0';
            if (number == Int64MaximumThreshold && lastDigit > 7)
                return IntegerParseResult.Overflow;
            number = number * 10 + lastDigit;

            // 20th digit produces an overflow, no matter what
            if (++bytesConsumed == source.Length)
                return IntegerParseResult.Success;

            currentCharacter = source[bytesConsumed];
            return !currentCharacter.IsJsonDigit() ? IntegerParseResult.Success : IntegerParseResult.Overflow;
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
                    else
                        return IntegerParseResult.Success;
                }

                return IntegerParseResult.Success;
            }

            for (++bytesConsumed; bytesConsumed < targetIndex; ++bytesConsumed)
            {
                currentCharacter = source[bytesConsumed];
                if (currentCharacter.IsJsonDigit())
                    number = number * 10 - (currentCharacter - '0');
                else
                    return IntegerParseResult.Success;
            }

            /* 19th digit is tricky:
             *  - long.MinValue is -9,223,372,036,854,775,808
             *  - it definitely produces an overflow if number is less than long.MinValue / 10
             *  - it produces an overflow if number is equal to long.MinValue / 10 and the last digit is greater than 7 (last digit of long.MaxValue)
             */
             if (bytesConsumed == source.Length)
                return IntegerParseResult.Success;

            currentCharacter = source[bytesConsumed];
            if (!currentCharacter.IsJsonDigit())
                return IntegerParseResult.Success;

            if (number < Int64MinimumThreshold)
                return IntegerParseResult.Overflow;

            var lastDigit = currentCharacter - '0';
            if (number == Int64MinimumThreshold && lastDigit > 8)
                return IntegerParseResult.Overflow;
            number = number * 10 - lastDigit;

            // 20th digit produces an overflow, no matter what
            if (++bytesConsumed == source.Length)
                return IntegerParseResult.Success;

            currentCharacter = source[bytesConsumed];
            return !currentCharacter.IsJsonDigit() ? IntegerParseResult.Success : IntegerParseResult.Overflow;
        }
    }
}
