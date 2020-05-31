using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Light.Json.Buffers;

namespace Light.Json.Serialization.LowLevelWriting
{
    public static class AsyncJsonWriterExtensions
    {
        public static async Task WriteStringAsync<TBufferWriter>(this IAsyncJsonWriter<TBufferWriter> jsonWriter, string? @string)
            where TBufferWriter : struct, IBufferWriter
        {
            if (@string == null)
            {
                await jsonWriter.BufferWriter.EnsureCapacityFromCurrentIndexAsync(4);
                jsonWriter.BufferWriter.WriteAscii('n');
                jsonWriter.BufferWriter.WriteAscii('u');
                jsonWriter.BufferWriter.WriteAscii('l');
                jsonWriter.BufferWriter.WriteAscii('l');
                return;
            }

            await jsonWriter.BufferWriter.EnsureCapacityFromCurrentIndexAsync(@string.Length + 2);

            jsonWriter.BufferWriter.WriteAscii('\"');
            for (var i = 0; i < @string.Length; ++i)
            {
                var character = @string[i];
                switch (character)
                {
                    case '"':
                    case '\\':
                    case '\b':
                    case '\f':
                    case '\n':
                    case '\r':
                    case '\t':
                        await jsonWriter.BufferWriter.EnsureAdditionalCapacityAsync(1);
                        jsonWriter.BufferWriter.WriteAscii('\\');
                        jsonWriter.BufferWriter.WriteAscii(character);
                        break;
                    default:

                        if (char.IsHighSurrogate(character))
                        {
                            if (++i == @string.Length)
                                throw new SerializationException("The following UTF-16 string ends with a high surrogate and cannot be processed (the low surrogate is missing):" + Environment.NewLine + "\"" + @string + "\"");

                            var lowSurrogate = @string[i];
                            if (!char.IsLowSurrogate(lowSurrogate))
                                throw new SerializationException($"In the following UTF-16 string, the high surrogate \"{(int) character:X4}\" at index {i - 1} is not followed by a low surrogate: {Environment.NewLine}{@string}");

                            await jsonWriter.BufferWriter.WriteSurrogatePairAsync(character, lowSurrogate);
                            break;
                        }

                        await jsonWriter.BufferWriter.WriteCharacterAsync(character);
                        break;
                }
            }

            jsonWriter.BufferWriter.WriteAscii('\"');
        }

        public static Task WriteIntegerAsync<TBufferWriter>(this IAsyncJsonWriter<TBufferWriter> jsonWriter, int number)
            where TBufferWriter : struct, IBufferWriter
        {
            var numberOfBufferSlots = 0;
            var isNegative = number < 0;
            if (isNegative)
            {
                if (number == int.MinValue) // This needs to be done because "int.MinValue * -1 = int.MinValue" in unchecked mode
                    return WriteInt32MinValueAsync(jsonWriter);

                ++numberOfBufferSlots;
                number *= -1;
            }

            var absoluteNumber = (uint) number;
            numberOfBufferSlots += DetermineNumberOfDigits(absoluteNumber);
            var capacityTask = jsonWriter.BufferWriter.EnsureCapacityFromCurrentIndexAsync(numberOfBufferSlots);
            if (capacityTask.Status != TaskStatus.RanToCompletion)
                return AwaitCapacityForInt32Async(capacityTask, jsonWriter, absoluteNumber, numberOfBufferSlots, isNegative);

            if (isNegative)
            {
                jsonWriter.BufferWriter.WriteAscii('-');
                --numberOfBufferSlots;
            }

            WriteUInt32Internal(jsonWriter, absoluteNumber, numberOfBufferSlots);
            return Task.CompletedTask;
        }

        private static async Task AwaitCapacityForInt32Async<TBufferWriter>(Task capacityTask,
                                                                            IAsyncJsonWriter<TBufferWriter> jsonWriter,
                                                                            uint absoluteNumber,
                                                                            int numberOfBufferSlots,
                                                                            bool isNegative)
            where TBufferWriter : struct, IBufferWriter
        {
            await capacityTask.ConfigureAwait(false);
            if (isNegative)
            {
                jsonWriter.BufferWriter.WriteAscii('-');
                --numberOfBufferSlots;
            }

            WriteUInt32Internal(jsonWriter, absoluteNumber, numberOfBufferSlots);
        }

        private static void WriteUInt32Internal<TBufferWriter>(IAsyncJsonWriter<TBufferWriter> jsonWriter, uint number, int numberOfDigits)
            where TBufferWriter : struct, IBufferWriter
        {
            ref var writer = ref jsonWriter.BufferWriter;
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

        private static Task WriteInt32MinValueAsync<TBufferWriter>(IAsyncJsonWriter<TBufferWriter> jsonWriter)
            where TBufferWriter : struct, IBufferWriter
        {
            var capacityTask = jsonWriter.BufferWriter.EnsureCapacityFromCurrentIndexAsync(11);
            if (capacityTask.Status != TaskStatus.RanToCompletion)
                return AwaitCapacityForInt32MinValueAsync(capacityTask, jsonWriter);

            WriteInt32MinValue(jsonWriter);
            return Task.CompletedTask;
        }

        private static async Task AwaitCapacityForInt32MinValueAsync<TBufferWriter>(Task capacityTask, IAsyncJsonWriter<TBufferWriter> jsonWriter)
            where TBufferWriter : struct, IBufferWriter
        {
            await capacityTask.ConfigureAwait(false);
            // ReSharper disable once MethodHasAsyncOverload
            WriteInt32MinValue(jsonWriter);
        }

        private static void WriteInt32MinValue<TBufferWriter>(IAsyncJsonWriter<TBufferWriter> jsonWriter)
            where TBufferWriter : struct, IBufferWriter
        {
            ref var writer = ref jsonWriter.BufferWriter;
            // -2,147,483,648
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