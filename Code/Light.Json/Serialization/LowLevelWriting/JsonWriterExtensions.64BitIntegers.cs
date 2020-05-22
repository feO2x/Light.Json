using System;

namespace Light.Json.Serialization.LowLevelWriting
{
    public static partial class JsonWriterExtensions
    {
        public static void WriteInteger<TJsonWriter>(this ref TJsonWriter writer, long number)
            where TJsonWriter : struct, IJsonWriter
        {
            var numberOfBufferSlots = 0;
            var isNegative = number < 0;
            if (isNegative)
            {
                if (number == long.MinValue) // This needs to be done because "long.MinValue * -1 = long.MinValue" in unchecked mode
                {
                    WriteInt64MinValue(ref writer);
                    return;
                }

                ++numberOfBufferSlots;
                number *= -1;
            }

            var absoluteNumber = (ulong) number;
            numberOfBufferSlots += DetermineNumberOfDigits(absoluteNumber);
            writer.EnsureCapacityFromCurrentIndex(numberOfBufferSlots);
            if (isNegative)
            {
                writer.WriteAscii('-');
                --numberOfBufferSlots;
            }

            writer.WriteUInt64Internal(absoluteNumber, numberOfBufferSlots);
        }

        public static void WriteInteger<TJsonWriter>(this ref TJsonWriter writer, ulong number)
            where TJsonWriter : struct, IJsonWriter
        {
            var numberOfBufferSlots = DetermineNumberOfDigits(number);
            writer.EnsureCapacityFromCurrentIndex(numberOfBufferSlots);
            writer.WriteUInt64Internal(number, numberOfBufferSlots);
        }

        private static void WriteUInt64Internal<TJsonWriter>(this ref TJsonWriter writer, ulong number, int numberOfDigits)
            where TJsonWriter : struct, IJsonWriter
        {
            if (numberOfDigits == 1)
            {
                writer.WriteAscii(number.ToDigitCharacter());
                return;
            }

            var divisor = (ulong) Math.Pow(10, numberOfDigits - 1);
            while (divisor >= 10)
            {
                var frontDigit = number / divisor;
                writer.WriteAscii(frontDigit.ToDigitCharacter());
                number -= frontDigit * divisor;
                divisor /= 10;
            }

            writer.WriteAscii(number.ToDigitCharacter());
        }

        private static int DetermineNumberOfDigits(this ulong number)
        {
            // 18,446,744,073,709,551,615 - maximum of 20 digits
            if (number <= uint.MaxValue)
                return DetermineNumberOfDigits((uint) number);

            if (number < 10_000_000_000)
                return 10;
            if (number < 100_000_000_000)
                return 11;
            if (number < 1_000_000_000_000)
                return 12;
            if (number < 10_000_000_000_000)
                return 13;
            if (number < 100_000_000_000_000)
                return 14;
            if (number < 100_000_000_000_000)
                return 14;
            if (number < 1_000_000_000_000_000)
                return 15;
            if (number < 10_000_000_000_000_000)
                return 16;
            if (number < 100_000_000_000_000_000)
                return 17;
            if (number < 1_000_000_000_000_000_000)
                return 18;
            if (number < 10_000_000_000_000_000_000)
                return 19;

            return 20;
        }

        private static void WriteInt64MinValue<TJsonWriter>(ref TJsonWriter writer) where TJsonWriter : struct, IJsonWriter
        {
            // -9,223,372,036,854,775,808
            writer.EnsureCapacityFromCurrentIndex(20);
            writer.WriteAscii('-');
            writer.WriteAscii('9');

            // Quadrillions
            writer.WriteAscii('2');
            writer.WriteAscii('2');
            writer.WriteAscii('3');

            // Trillions
            writer.WriteAscii('3');
            writer.WriteAscii('7');
            writer.WriteAscii('2');

            // Billions
            writer.WriteAscii('0');
            writer.WriteAscii('3');
            writer.WriteAscii('6');

            // Millions
            writer.WriteAscii('8');
            writer.WriteAscii('5');
            writer.WriteAscii('4');

            // Thousands
            writer.WriteAscii('7');
            writer.WriteAscii('7');
            writer.WriteAscii('5');

            // Hundreds
            writer.WriteAscii('8');
            writer.WriteAscii('0');
            writer.WriteAscii('8');
        }

        private static char ToDigitCharacter(this ulong number) => (char) (number + '0');
    }
}