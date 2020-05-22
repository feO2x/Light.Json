﻿using System;

namespace Light.Json.Serialization.LowLevelWriting
{
    public static partial class JsonWriterExtensions
    {
        public static void WriteInteger<TJsonWriter>(this ref TJsonWriter writer, int number)
            where TJsonWriter : struct, IJsonWriter
        {
            var numberOfBufferSlots = 0;
            var isNegative = number < 0;
            if (isNegative)
            {
                if (number == int.MinValue) // This needs to be done because "int.MinValue * -1 = int.MinValue" in unchecked mode
                {
                    WriteInt32MinValue(ref writer);
                    return;
                }

                ++numberOfBufferSlots;
                number *= -1;
            }

            var absoluteNumber = (uint) number;
            numberOfBufferSlots += DetermineNumberOfDigits(absoluteNumber);
            writer.EnsureCapacityFromCurrentIndex(numberOfBufferSlots);
            if (isNegative)
            {
                writer.WriteAscii('-');
                --numberOfBufferSlots;
            }

            writer.WriteUInt32Internal(absoluteNumber, numberOfBufferSlots);
        }

        public static void WriteInteger<TJsonWriter>(this ref TJsonWriter writer, uint number)
            where TJsonWriter : struct, IJsonWriter
        {
            var numberOfBufferSlots = DetermineNumberOfDigits(number);
            writer.EnsureCapacityFromCurrentIndex(numberOfBufferSlots);
            writer.WriteUInt32Internal(number, numberOfBufferSlots);
        }

        private static void WriteUInt32Internal<TJsonWriter>(this ref TJsonWriter writer, uint number, int numberOfDigits)
            where TJsonWriter : struct, IJsonWriter
        {
            if (numberOfDigits == 1)
            {
                writer.WriteAscii(number.ToDigitCharacter());
                return;
            }

            var divisor = (uint) Math.Pow(10, numberOfDigits - 1);
            while (divisor >= 10)
            {
                var frontDigit = number / divisor;
                writer.WriteAscii(frontDigit.ToDigitCharacter());
                number -= frontDigit * divisor;
                divisor /= 10;
            }

            writer.WriteAscii(number.ToDigitCharacter());
        }

        private static void WriteInt32MinValue<TJsonWriter>(ref TJsonWriter writer)
            where TJsonWriter : struct, IJsonWriter
        {
            // -2,147,483,648
            writer.EnsureCapacityFromCurrentIndex(11);
            writer.WriteAscii('-');
            writer.WriteAscii('2');

            // Millions
            writer.WriteAscii('1');
            writer.WriteAscii('4');
            writer.WriteAscii('7');

            // Thousands
            writer.WriteAscii('4');
            writer.WriteAscii('8');
            writer.WriteAscii('3');

            // Hundreds
            writer.WriteAscii('6');
            writer.WriteAscii('4');
            writer.WriteAscii('8');
        }

        private static int DetermineNumberOfDigits(this uint number)
        {
            // uint.MaxValue is 4,294,967,295
            if (number < 10)
                return 1;
            if (number < 100)
                return 2;
            if (number < 1000)
                return 3;
            if (number < 10_000)
                return 4;
            if (number < 100_000)
                return 5;
            if (number < 1_000_000)
                return 6;
            if (number < 10_000_000)
                return 7;
            if (number < 100_000_000)
                return 8;
            if (number < 1_000_000_000)
                return 9;

            return 10;
        }

        private static char ToDigitCharacter(this uint number) => (char) (number + '0');
    }
}