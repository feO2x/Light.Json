using System.Runtime.CompilerServices;

namespace Light.Json.Serialization.LowLevelWriting
{
    public static partial class JsonWriterExtensions
    {
        public static void WriteInteger<TJsonWriter>(this ref TJsonWriter writer, long number)
            where TJsonWriter : struct, IJsonWriter
        {
            if (number < 0)
            {
                if (number == long.MinValue)
                {
                    WriteInt64MinValue(ref writer);
                    return;
                }

                writer.EnsureCapacityFromCurrentIndex(1);
                writer.WriteAscii('-');
                number = -number;
            }

            WriteInteger(ref writer, (ulong) number);
        }

        public static void WriteInteger<TJsonWriter>(this ref TJsonWriter writer, ulong number)
            where TJsonWriter : struct, IJsonWriter
        {
            ulong div;

            if (number < 10000)
            {
                if (number < 10)
                {
                    writer.EnsureCapacityFromCurrentIndex(1);
                    goto Digit1;
                }

                if (number < 100)
                {
                    writer.EnsureCapacityFromCurrentIndex(2);
                    goto Digit2;
                }

                if (number < 1000)
                {
                    writer.EnsureCapacityFromCurrentIndex(3);
                    goto Digit3;
                }

                writer.EnsureCapacityFromCurrentIndex(4);
                goto Digit4;
            }

            var number2 = number / 10000;
            number -= number2 * 10000;
            if (number2 < 10000)
            {
                if (number2 < 10)
                {
                    writer.EnsureCapacityFromCurrentIndex(5);
                    goto Digit5;
                }

                if (number2 < 100)
                {
                    writer.EnsureCapacityFromCurrentIndex(6);
                    goto Digit6;
                }

                if (number2 < 1000)
                {
                    writer.EnsureCapacityFromCurrentIndex(7);
                    goto Digit7;
                }

                writer.EnsureCapacityFromCurrentIndex(8);
                goto Digit8;
            }

            var number3 = number2 / 10000;
            number2 -= number3 * 10000;
            if (number3 < 10000)
            {
                if (number3 < 10)
                {
                    writer.EnsureCapacityFromCurrentIndex(9);
                    goto Digit9;
                }

                if (number3 < 100)
                {
                    writer.EnsureCapacityFromCurrentIndex(10);
                    goto Digit10;
                }

                if (number3 < 1000)
                {
                    writer.EnsureCapacityFromCurrentIndex(11);
                    goto Digit11;
                }

                writer.EnsureCapacityFromCurrentIndex(12);
                goto Digit12;
            }

            var number4 = number3 / 10000;
            number3 -= number4 * 10000;
            if (number4 < 10000)
            {
                if (number4 < 10)
                {
                    writer.EnsureCapacityFromCurrentIndex(13);
                    goto Digit13;
                }

                if (number4 < 100)
                {
                    writer.EnsureCapacityFromCurrentIndex(14);
                    goto Digit14;
                }

                if (number4 < 1000)
                {
                    writer.EnsureCapacityFromCurrentIndex(15);
                    goto Digit15;
                }

                writer.EnsureCapacityFromCurrentIndex(16);
                goto Digit16;
            }

            var number5 = number4 / 10000;
            number4 -= number5 * 10000;
            if (number5 < 10000)
            {
                if (number5 < 10)
                {
                    writer.EnsureCapacityFromCurrentIndex(17);
                    goto Digit17;
                }

                if (number5 < 100)
                {
                    writer.EnsureCapacityFromCurrentIndex(18);
                    goto Digit18;
                }

                if (number5 < 1000)
                {
                    writer.EnsureCapacityFromCurrentIndex(19);
                    goto Digit19;
                }

                writer.EnsureCapacityFromCurrentIndex(20);
                writer.WriteAscii((div = (number5 * 8389UL) >> 23).ToDigitCharacter());
                number5 -= div * 1000;
            }

            Digit19:
            writer.WriteAscii((div = (number5 * 5243UL) >> 19).ToDigitCharacter());
            number5 -= div * 100;
            Digit18:
            writer.WriteAscii((div = (number5 * 6554UL) >> 16).ToDigitCharacter());
            number5 -= div * 10;
            Digit17:
            writer.WriteAscii(number5.ToDigitCharacter());
            Digit16:
            writer.WriteAscii((div = (number4 * 8389UL) >> 23).ToDigitCharacter());
            number4 -= div * 1000;
            Digit15:
            writer.WriteAscii((div = (number4 * 5243UL) >> 19).ToDigitCharacter());
            number4 -= div * 100;
            Digit14:
            writer.WriteAscii((div = (number4 * 6554UL) >> 16).ToDigitCharacter());
            number4 -= div * 10;
            Digit13:
            writer.WriteAscii((number4).ToDigitCharacter());
            Digit12:
            writer.WriteAscii((div = (number3 * 8389UL) >> 23).ToDigitCharacter());
            number3 -= div * 1000;
            Digit11:
            writer.WriteAscii((div = (number3 * 5243UL) >> 19).ToDigitCharacter());
            number3 -= div * 100;
            Digit10:
            writer.WriteAscii((div = (number3 * 6554UL) >> 16).ToDigitCharacter());
            number3 -= div * 10;
            Digit9:
            writer.WriteAscii(number3.ToDigitCharacter());
            Digit8:
            writer.WriteAscii((div = (number2 * 8389UL) >> 23).ToDigitCharacter());
            number2 -= div * 1000;
            Digit7:
            writer.WriteAscii((div = (number2 * 5243UL) >> 19).ToDigitCharacter());
            number2 -= div * 100;
            Digit6:
            writer.WriteAscii((div = (number2 * 6554UL) >> 16).ToDigitCharacter());
            number2 -= div * 10;
            Digit5:
            writer.WriteAscii((number2).ToDigitCharacter());
            Digit4:
            writer.WriteAscii((div = (number * 8389UL) >> 23).ToDigitCharacter());
            number -= div * 1000;
            Digit3:
            writer.WriteAscii((div = (number * 5243UL) >> 19).ToDigitCharacter());
            number -= div * 100;
            Digit2:
            writer.WriteAscii((div = (number * 6554UL) >> 16).ToDigitCharacter());
            number -= div * 10;
            Digit1:
            writer.WriteAscii(number.ToDigitCharacter());
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static char ToDigitCharacter(this ulong number) => (char) (number + '0');
    }
}