﻿using System;
using System.Runtime.CompilerServices;
using System.Text;
using Light.GuardClauses;
using Light.Json.Buffers;
using Light.Json.Serialization.LowLevelWriting;

namespace Light.Json.Tests.Serialization.JsonWriterPerformance
{
    public struct JsonWriter
    {
        public JsonWriter(IBufferProvider<byte> bufferProvider)
        {
            BufferProvider = bufferProvider.MustNotBeNull(nameof(bufferProvider));
            CurrentBuffer = bufferProvider.GetInitialBuffer();
            CurrentIndex = 0;
        }

        public IBufferProvider<byte> BufferProvider { get; }

        public byte[] CurrentBuffer { get; private set; }

        public int CurrentIndex { get; private set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteCharacter(char character) => WriteByte((byte) character);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteByte(byte character) => CurrentBuffer[CurrentIndex++] = character;

        public unsafe void WriteString(ReadOnlySpan<char> value)
        {
            EnsureCapacity(Encoding.UTF8.GetMaxByteCount(value.Length) + 2);
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
            EnsureCapacity(bytes.Length);
            fixed (void* source = &bytes[0], target = &CurrentBuffer[CurrentIndex])
                Buffer.MemoryCopy(source, target, CurrentBuffer.Length - CurrentIndex, bytes.Length);
            CurrentIndex += bytes.Length;
        }

        public void WriteNumber(long number)
        {
            if (number < 0)
            {
                if (number == int.MinValue) // This needs to be done because "int.MinValue * -1 = int.MinValue" in unchecked mode
                {
                    WriteInt32MinValue();
                    return;
                }

                EnsureCapacity(1);
                WriteCharacter('-');
                number = -number;
            }

            WriteUInt32Internal((ulong) number);
        }

        private void WriteUInt32Internal(ulong number)
        {
            ulong div;

            if (number < 10000)
            {
                if (number < 10)
                {
                    EnsureCapacity(1);
                    goto Digit1;
                }

                if (number < 100)
                {
                    EnsureCapacity(2);
                    goto Digit2;
                }

                if (number < 1000)
                {
                    EnsureCapacity(3);
                    goto Digit3;
                }

                EnsureCapacity(4);
                goto Digit4;
            }

            var number2 = number / 10000;
            number -= number2 * 10000;
            if (number2 < 10000)
            {
                if (number2 < 10)
                {
                    EnsureCapacity(5);
                    goto Digit5;
                }

                if (number2 < 100)
                {
                    EnsureCapacity(6);
                    goto Digit6;
                }

                if (number2 < 1000)
                {
                    EnsureCapacity(7);
                    goto Digit7;
                }

                EnsureCapacity(8);
                goto Digit8;
            }

            var number3 = number2 / 10000;
            number2 -= number3 * 10000;
            if (number3 < 10000)
            {
                if (number3 < 10)
                {
                    EnsureCapacity(9);
                    goto Digit9;
                }

                if (number3 < 100)
                {
                    EnsureCapacity(10);
                    goto Digit10;
                }

                if (number3 < 1000)
                {
                    EnsureCapacity(11);
                    goto Digit11;
                }

                EnsureCapacity(12);
                goto Digit12;
            }

            var number4 = number3 / 10000;
            number3 -= number4 * 10000;
            if (number4 < 10000)
            {
                if (number4 < 10)
                {
                    EnsureCapacity(13);
                    goto Digit13;
                }

                if (number4 < 100)
                {
                    EnsureCapacity(14);
                    goto Digit14;
                }

                if (number4 < 1000)
                {
                    EnsureCapacity(15);
                    goto Digit15;
                }

                EnsureCapacity(16);
                goto Digit16;
            }

            var number5 = number4 / 10000;
            number4 -= number5 * 10000;
            if (number5 < 10000)
            {
                if (number5 < 10)
                {
                    EnsureCapacity(17);
                    goto Digit17;
                }

                if (number5 < 100)
                {
                    EnsureCapacity(18);
                    goto Digit18;
                }

                if (number5 < 1000)
                {
                    EnsureCapacity(19);
                    goto Digit19;
                }

                EnsureCapacity(20);
                WriteByte((div = (number5 * 8389UL) >> 23).ToUtf8DigitCharacter());
                number5 -= div * 1000;
            }

            Digit19:
            WriteByte((div = (number5 * 5243UL) >> 19).ToUtf8DigitCharacter());
            number5 -= div * 100;
            Digit18:
            WriteByte((div = (number5 * 6554UL) >> 16).ToUtf8DigitCharacter());
            number5 -= div * 10;
            Digit17:
            WriteByte(number5.ToUtf8DigitCharacter());
            Digit16:
            WriteByte((div = (number4 * 8389UL) >> 23).ToUtf8DigitCharacter());
            number4 -= div * 1000;
            Digit15:
            WriteByte((div = (number4 * 5243UL) >> 19).ToUtf8DigitCharacter());
            number4 -= div * 100;
            Digit14:
            WriteByte((div = (number4 * 6554UL) >> 16).ToUtf8DigitCharacter());
            number4 -= div * 10;
            Digit13:
            WriteByte((number4).ToUtf8DigitCharacter());
            Digit12:
            WriteByte((div = (number3 * 8389UL) >> 23).ToUtf8DigitCharacter());
            number3 -= div * 1000;
            Digit11:
            WriteByte((div = (number3 * 5243UL) >> 19).ToUtf8DigitCharacter());
            number3 -= div * 100;
            Digit10:
            WriteByte((div = (number3 * 6554UL) >> 16).ToUtf8DigitCharacter());
            number3 -= div * 10;
            Digit9:
            WriteByte(number3.ToUtf8DigitCharacter());
            Digit8:
            WriteByte((div = (number2 * 8389UL) >> 23).ToUtf8DigitCharacter());
            number2 -= div * 1000;
            Digit7:
            WriteByte((div = (number2 * 5243UL) >> 19).ToUtf8DigitCharacter());
            number2 -= div * 100;
            Digit6:
            WriteByte((div = (number2 * 6554UL) >> 16).ToUtf8DigitCharacter());
            number2 -= div * 10;
            Digit5:
            WriteByte((number2).ToUtf8DigitCharacter());
            Digit4:
            WriteByte((div = (number * 8389UL) >> 23).ToUtf8DigitCharacter());
            number -= div * 1000;
            Digit3:
            WriteByte((div = (number * 5243UL) >> 19).ToUtf8DigitCharacter());
            number -= div * 100;
            Digit2:
            WriteByte((div = (number * 6554UL) >> 16).ToUtf8DigitCharacter());
            number -= div * 10;
            Digit1:
            WriteByte(number.ToUtf8DigitCharacter());
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

        public void WriteEndOfObject() => WriteCharacter('}');

        public SerializationResult<byte> GetUtf8Json() =>
            new SerializationResult<byte>(new Memory<byte>(CurrentBuffer, 0, CurrentIndex), CurrentBuffer, BufferProvider);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureCapacity(int numberOfSlots)
        {
            var newSize = CurrentIndex + numberOfSlots;
            if (newSize < CurrentBuffer.Length)
                return;

            CurrentBuffer = BufferProvider.GetNewBufferWithIncreasedSize(CurrentBuffer, newSize - CurrentBuffer.Length);
        }
    }
}