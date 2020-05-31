using System;
using System.Threading.Tasks;
using Light.GuardClauses;
using Light.Json.FrameworkExtensions;

namespace Light.Json.Buffers
{
    public struct Utf8BufferWriter : IBufferWriter, IBufferWriterState<byte>
    {
        public Utf8BufferWriter(IBufferWriterService<byte> bufferWriterService)
        {
            BufferWriterService = bufferWriterService.MustNotBeNull(nameof(bufferWriterService));
            CurrentBuffer = bufferWriterService.GetInitialBuffer();
            CurrentIndex = EnsuredIndex = 0;
        }

        public IBufferWriterService<byte> BufferWriterService { get; }
        public int CurrentIndex { get; set; }
        public int EnsuredIndex { get; set; }
        public byte[] CurrentBuffer { get; set; }

        public Task EnsureCapacityFromCurrentIndexAsync(int numberOfBufferSlots)
        {
            numberOfBufferSlots.MustBeGreaterThan(0, nameof(numberOfBufferSlots));
            EnsuredIndex = CurrentIndex + numberOfBufferSlots;
            return EnsureCapacityAsync();
        }

        public Task EnsureAdditionalCapacityAsync(int numberOfAdditionalBufferSlots)
        {
            numberOfAdditionalBufferSlots.MustBeGreaterThan(0, nameof(numberOfAdditionalBufferSlots));
            EnsuredIndex += numberOfAdditionalBufferSlots;
            return EnsureCapacityAsync();
        }

        public Task WriteCharacterAsync(char character)
        {
            if (character < 128)
            {
                WriteByte((byte) character);
                return Task.CompletedTask;
            }

            return character < 2048 ? WriteTwoByteCharacterAsync(character) : WriteThreeByteCharacterAsync(character);
        }

        public void WriteAscii(char character) => WriteByte((byte) character);

        public Task WriteSurrogatePairAsync(char highSurrogate, char lowSurrogate)
        {
            var codePoint = char.ConvertToUtf32(highSurrogate, lowSurrogate);
            return codePoint < 65536 ? WriteThreeByteCharacterAsync(codePoint) : WriteFourByteCharacterAsync(codePoint);
        }

        public override string ToString() =>
            new Span<byte>(CurrentBuffer, 0, CurrentIndex).ConvertFromUtf8ToString();

        public byte[] Finish()
        {
            var array = new Span<byte>(CurrentBuffer, 0, CurrentIndex).ToArray();
            BufferWriterService.ReturnBuffer(CurrentBuffer);
            return array;
        }

        private void WriteByte(byte character) => CurrentBuffer[CurrentIndex++] = character;

        private Task WriteTwoByteCharacterAsync(int character)
        {
            var capacityTask = EnsureAdditionalCapacityAsync(1);
            if (capacityTask.Status != TaskStatus.RanToCompletion)
                return AwaitCapacityForTwoByteCharacterAsync(capacityTask, character);

            WriteTwoByteCharacter(character);
            return Task.CompletedTask;
        }

        private async Task AwaitCapacityForTwoByteCharacterAsync(Task capacityTask, int character)
        {
            await capacityTask.ConfigureAwait(false);
            // ReSharper disable once MethodHasAsyncOverload
            WriteTwoByteCharacter(character);
        }

        private void WriteTwoByteCharacter(int character)
        {
            // The first byte holds the upper 5 bits, prefixed with binary 110
            var firstByte = (byte) (0b1100_0000 | (character >> 6)); // The lower 6 bit are shifted out
            // The second byte holds the lower 6 bits, prefixed with binary 10
            var secondByte = (byte) (0b1000_0000 | (character & 0b0011_1111));
            WriteByte(firstByte);
            WriteByte(secondByte);
        }

        private Task WriteThreeByteCharacterAsync(int character)
        {
            var capacityTask = EnsureAdditionalCapacityAsync(2);
            if (capacityTask.Status != TaskStatus.RanToCompletion)
                return AwaitCapacityForThreeByteCharacterAsync(capacityTask, character);

            WriteThreeByteCharacter(character);
            return Task.CompletedTask;
        }

        private async Task AwaitCapacityForThreeByteCharacterAsync(Task capacityTask, int character)
        {
            await capacityTask.ConfigureAwait(false);
            // ReSharper disable once MethodHasAsyncOverload
            WriteThreeByteCharacter(character);
        }

        private void WriteThreeByteCharacter(int character)
        {
            // The first byte holds the upper 4 bits, prefixed with binary 1110
            var firstByte = (byte) (0b1110_0000 | (character >> 12)); // The lower 12 bits are shifted out
            var secondByte = (byte) (0b1000_0000 | ((character >> 6) & 0b0011_1111)); // Take bits 7 to 12 and put them in the second byte
            var thirdByte = (byte) (0b1000_0000 | (character & 0b0011_1111)); // Take the lowest six bits and put them in the third byte
            WriteByte(firstByte);
            WriteByte(secondByte);
            WriteByte(thirdByte);
        }

        private Task WriteFourByteCharacterAsync(int character)
        {
            var capacityTask = EnsureAdditionalCapacityAsync(3);
            if (capacityTask.Status != TaskStatus.RanToCompletion)
                return AwaitCapacityForFourByteCharacterAsync(capacityTask, character);

            WriteFourByteCharacter(character);
            return Task.CompletedTask;
        }

        private async Task AwaitCapacityForFourByteCharacterAsync(Task capacityTask, int character)
        {
            await capacityTask.ConfigureAwait(false);
            // ReSharper disable once MethodHasAsyncOverload
            WriteFourByteCharacter(character);
        }

        private void WriteFourByteCharacter(int character)
        {
            var firstByte = (byte) (0b11110_000 | (character >> 18));
            var secondByte = (byte) (0b1000_0000 | ((character >> 12) & 0b0011_1111));
            var thirdByte = (byte) (0b1000_0000 | ((character >> 6) & 0b0011_1111));
            var fourthByte = (byte) (0b1000_0000 | (character & 0b0011_1111));
            WriteByte(firstByte);
            WriteByte(secondByte);
            WriteByte(thirdByte);
            WriteByte(fourthByte);
        }

        private Task EnsureCapacityAsync()
        {
            if (EnsuredIndex < CurrentBuffer.Length)
                return Task.CompletedTask;

            var bufferState = this.GetBufferState<Utf8BufferWriter, byte>();
            var processTask = BufferWriterService.ProcessFullBufferAsync(bufferState, EnsuredIndex - CurrentIndex - 1);
            if (!processTask.IsCompletedSuccessfully)
                return AwaitNewBufferStateAsync(processTask);

            this.ApplyBufferState(processTask.Result);
            return Task.CompletedTask;
        }

        private async Task AwaitNewBufferStateAsync(ValueTask<BufferState<byte>> processTask)
        {
            var bufferState = await processTask.ConfigureAwait(false);
            this.ApplyBufferState(bufferState);
        }
    }
}