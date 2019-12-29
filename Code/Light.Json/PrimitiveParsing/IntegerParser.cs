using System;

namespace Light.Json.PrimitiveParsing
{
    public static class IntegerParser
    {
        public static IntegerParseResult TryParseInt64(this ReadOnlySpan<byte> utf8Source, out long value, out int bytesConsumed)
        {
            if (utf8Source.Length < 1)
            {
                bytesConsumed = 0;
                value = default;
                return IntegerParseResult.NoNumber;
            }

            // Check the sign
            var currentIndex = 0;
            var sign = 1;
            if (utf8Source[0] == '-')
            {
                currentIndex = 1;
                sign = -1;

                if (utf8Source.Length <= currentIndex)
                {
                    bytesConsumed = 0;
                    value = default;
                    return IntegerParseResult.NoNumber;
                }
            }
            else if (utf8Source[0] == '+')
            {
                currentIndex = 1;

                if (utf8Source.Length <= currentIndex)
                {
                    bytesConsumed = 0;
                    value = default;
                    return IntegerParseResult.NoNumber;
                }
            }

            var currentDigit = utf8Source[currentIndex] - '0';
            if (currentDigit < 0 || currentDigit > 9)
            {
                bytesConsumed = 0;
                value = default;
                return IntegerParseResult.NoNumber;
            }

            var parsedValue = (ulong) currentDigit;

            // Check if the source cannot overflow
            var overflowIndex = currentIndex + 19; // a 64 bit integer can consist of at most 19 digits
            if (utf8Source.Length < overflowIndex)
            {
                for (++currentIndex; currentIndex < utf8Source.Length; ++currentIndex)
                {
                    currentDigit = utf8Source[currentIndex] - '0';
                    if (currentDigit < 0 || currentDigit > 9)
                    {
                        bytesConsumed = currentIndex;
                        value = (long) parsedValue * sign;
                        return IntegerParseResult.ParsingSuccessful;
                    }

                    parsedValue = parsedValue * 10 + (ulong) currentDigit;
                }
            }
            else
            {
                // In this branch, a potential overflow is possible. We can safely add up all digits until we reach the overflow index
                // Length is greater than Parsers.Int64OverflowLength; overflow is only possible after Parsers.Int64OverflowLength
                // digits. There may be no overflow after Parsers.Int64OverflowLength if there are leading zeroes.
                for (++currentIndex; currentIndex < overflowIndex - 1; ++currentIndex)
                {
                    currentDigit = utf8Source[currentIndex] - '0';
                    if (currentDigit < 0 || currentDigit > 9)
                    {
                        bytesConsumed = currentIndex;
                        value = (long) parsedValue * sign;
                        return IntegerParseResult.ParsingSuccessful;
                    }

                    parsedValue = parsedValue * 10 + (ulong) currentDigit;
                }

                // Up here is the tricky one: we need to update only when we do not exceed long.MaxValue or long.MinValue
                var isPositive = sign > 0;

                for (currentIndex = overflowIndex - 1; currentIndex < utf8Source.Length; ++currentIndex)
                {
                    currentDigit = utf8Source[currentIndex] - '0';
                    if (currentDigit < 0 || currentDigit > 9)
                    {
                        bytesConsumed = currentIndex;
                        value = (long) parsedValue * sign;
                        return IntegerParseResult.ParsingSuccessful;
                    }

                    // The value can only overflow when current digit is greater than 7 (positive) or greater than 8 (negative)
                    var isNextDigitTooLarge = isPositive ? currentDigit > 7 : currentDigit > 8;
                    if (parsedValue > long.MaxValue / 10 || parsedValue == long.MaxValue / 10 && isNextDigitTooLarge)
                    {
                        bytesConsumed = 0;
                        value = default;
                        return IntegerParseResult.Overflow;
                    }

                    parsedValue = parsedValue * 10 + (ulong) currentDigit;
                }
            }

            // If this part is reached, parsing was successful
            bytesConsumed = utf8Source.Length;
            value = (long) parsedValue * sign;
            return IntegerParseResult.ParsingSuccessful;
        }

        public static IntegerParseResult TryParseInt64(this ReadOnlySpan<char> source, out long value, out int bytesConsumed)
        {
            if (source.Length < 1)
            {
                bytesConsumed = 0;
                value = default;
                return IntegerParseResult.NoNumber;
            }

            // Check the sign
            var currentIndex = 0;
            var sign = 1;
            if (source[0] == '-')
            {
                currentIndex = 1;
                sign = -1;

                if (source.Length <= currentIndex)
                {
                    bytesConsumed = 0;
                    value = default;
                    return IntegerParseResult.NoNumber;
                }
            }
            else if (source[0] == '+')
            {
                currentIndex = 1;

                if (source.Length <= currentIndex)
                {
                    bytesConsumed = 0;
                    value = default;
                    return IntegerParseResult.NoNumber;
                }
            }

            var currentDigit = source[currentIndex] - '0';
            if (currentDigit < 0 || currentDigit > 9)
            {
                bytesConsumed = 0;
                value = default;
                return IntegerParseResult.NoNumber;
            }

            var parsedValue = (ulong) currentDigit;

            // Check if the source cannot overflow
            var overflowIndex = currentIndex + 19; // a 64 bit integer can consist of at most 19 digits
            if (source.Length < overflowIndex)
            {
                for (++currentIndex; currentIndex < source.Length; ++currentIndex)
                {
                    currentDigit = source[currentIndex] - '0';
                    if (currentDigit < 0 || currentDigit > 9)
                    {
                        bytesConsumed = currentIndex;
                        value = (long) parsedValue * sign;
                        return IntegerParseResult.ParsingSuccessful;
                    }

                    parsedValue = parsedValue * 10 + (ulong) currentDigit;
                }
            }
            else
            {
                // In this branch, a potential overflow is possible. We can safely add up all digits until we reach the overflow index
                // Length is greater than Parsers.Int64OverflowLength; overflow is only possible after Parsers.Int64OverflowLength
                // digits. There may be no overflow after Parsers.Int64OverflowLength if there are leading zeroes.
                for (++currentIndex; currentIndex < overflowIndex - 1; ++currentIndex)
                {
                    currentDigit = source[currentIndex] - '0';
                    if (currentDigit < 0 || currentDigit > 9)
                    {
                        bytesConsumed = currentIndex;
                        value = (long) parsedValue * sign;
                        return IntegerParseResult.ParsingSuccessful;
                    }

                    parsedValue = parsedValue * 10 + (ulong) currentDigit;
                }

                // Up here is the tricky one: we need to update only when we do not exceed long.MaxValue or long.MinValue
                var isPositive = sign > 0;

                for (currentIndex = overflowIndex - 1; currentIndex < source.Length; ++currentIndex)
                {
                    currentDigit = source[currentIndex] - '0';
                    if (currentDigit < 0 || currentDigit > 9)
                    {
                        bytesConsumed = currentIndex;
                        value = (long) parsedValue * sign;
                        return IntegerParseResult.ParsingSuccessful;
                    }

                    // The value can only overflow when current digit is greater than 7 (positive) or greater than 8 (negative)
                    var isNextDigitTooLarge = isPositive ? currentDigit > 7 : currentDigit > 8;
                    if (parsedValue > long.MaxValue / 10 || parsedValue == long.MaxValue / 10 && isNextDigitTooLarge)
                    {
                        bytesConsumed = 0;
                        value = default;
                        return IntegerParseResult.Overflow;
                    }

                    parsedValue = parsedValue * 10 + (ulong) currentDigit;
                }
            }

            // If this part is reached, parsing was successful
            bytesConsumed = source.Length;
            value = (long) parsedValue * sign;
            return IntegerParseResult.ParsingSuccessful;
        }
    }
}