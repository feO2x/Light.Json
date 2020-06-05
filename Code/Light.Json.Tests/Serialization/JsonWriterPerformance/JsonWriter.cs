using System;
using System.Text;
using Light.Json.Serialization.LowLevelWriting;

namespace Light.Json.Tests.Serialization.JsonWriterPerformance
{
    public struct JsonWriter
    {
        public JsonWriter(byte[] currentBuffer)
        {
            CurrentBuffer = currentBuffer;
            CurrentIndex = 0;
        }

        public byte[] CurrentBuffer { get; }

        public int CurrentIndex { get; private set; }

        public void WriteCharacter(char character) => CurrentBuffer[CurrentIndex++] = (byte) character;

        public unsafe void WriteString(ReadOnlySpan<char> value)
        {
            WriteCharacter('\"');
            fixed (char* source = &value[0])
            fixed (byte* target = &CurrentBuffer[CurrentIndex])
            {
                CurrentIndex += Encoding.UTF8.GetBytes(source, value.Length, target, CurrentBuffer.Length - CurrentIndex);
            }

            WriteCharacter('\"');
        }

        public unsafe void WriteRaw(ReadOnlySpan<byte> bytes)
        {
            fixed (void* source = &bytes[0], target = &CurrentBuffer[CurrentIndex])
                Buffer.MemoryCopy(source, target, CurrentBuffer.Length - CurrentIndex, bytes.Length);
            CurrentIndex += bytes.Length;
        }

        public void WriteNumber(int number)
        {
            var numberOfBufferSlots = 0;
            var isNegative = number < 0;
            if (isNegative)
            {
                if (number == int.MinValue) // This needs to be done because "int.MinValue * -1 = int.MinValue" in unchecked mode
                {
                    WriteInt32MinValue();
                    return;
                }

                ++numberOfBufferSlots;
                number *= -1;
            }

            var absoluteNumber = (uint) number;
            numberOfBufferSlots += absoluteNumber.DetermineNumberOfDigits();
            if (isNegative)
            {
                WriteCharacter('-');
                --numberOfBufferSlots;
            }

            WriteUInt32Internal(absoluteNumber, numberOfBufferSlots);
        }

        private void WriteUInt32Internal(uint number, int numberOfDigits)
        {
            if (numberOfDigits == 1)
            {
                WriteCharacter(number.ToDigitCharacter());
                return;
            }

            var divisor = (uint) Math.Pow(10, numberOfDigits - 1);
            while (divisor >= 10)
            {
                var frontDigit = number / divisor;
                WriteCharacter(frontDigit.ToDigitCharacter());
                number -= frontDigit * divisor;
                divisor /= 10;
            }

            WriteCharacter(number.ToDigitCharacter());
        }

        private void WriteInt32MinValue()
        {
            WriteCharacter('-');
            WriteCharacter('2');

            // Millions
            WriteCharacter('1');
            WriteCharacter('4');
            WriteCharacter('7');

            // Thousands
            WriteCharacter('4');
            WriteCharacter('8');
            WriteCharacter('3');

            // Hundreds
            WriteCharacter('6');
            WriteCharacter('4');
            WriteCharacter('8');
        }

        public void WriteEndOfObject()
        {
            WriteCharacter('}');
        }

        public Memory<byte> GetUtf8Json() =>
            new Memory<byte>(CurrentBuffer, 0, CurrentIndex);
    }
}