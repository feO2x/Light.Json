using System;
using System.Threading.Tasks;
using Light.GuardClauses;

namespace Light.Json.Buffers
{
    public struct Utf16BufferWriter : IBufferWriter, IBufferWriterState<char>
    {
        public Utf16BufferWriter(IBufferWriterService<char> bufferWriterService)
        {
            BufferWriterService = bufferWriterService.MustNotBeNull(nameof(bufferWriterService));
            CurrentBuffer = bufferWriterService.GetInitialBuffer();
            CurrentIndex = EnsuredIndex = 0;
        }

        public IBufferWriterService<char> BufferWriterService { get; }
        public char[] CurrentBuffer { get; set; }
        public int CurrentIndex { get; set; }
        public int EnsuredIndex { get; set; }

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
            WriteCharacter(character);
            return Task.CompletedTask;
        }

        public void WriteAscii(char character) => WriteCharacter(character);

        public Task WriteSurrogatePairAsync(char highSurrogate, char lowSurrogate)
        {
            WriteCharacter(highSurrogate);
            WriteCharacter(lowSurrogate);
            return Task.CompletedTask;
        }

        public override string ToString() => new Span<char>(CurrentBuffer, 0, CurrentIndex).ToString();

        public string Finish()
        {
            var @string = ToString();
            BufferWriterService.ReturnBuffer(CurrentBuffer);
            return @string;
        }

        private void WriteCharacter(char character) => CurrentBuffer[CurrentIndex++] = character;

        private Task EnsureCapacityAsync()
        {
            if (EnsuredIndex < CurrentBuffer.Length)
                return Task.CompletedTask;

            var bufferState = this.GetBufferState<Utf16BufferWriter, char>();
            var processTask = BufferWriterService.ProcessFullBufferAsync(bufferState, EnsuredIndex - CurrentIndex - 1);
            if (!processTask.IsCompletedSuccessfully)
                return AwaitNewBufferStateAsync(processTask);

            this.ApplyBufferState(processTask.Result);
            return Task.CompletedTask;
        }

        private async Task AwaitNewBufferStateAsync(ValueTask<BufferState<char>> processTask)
        {
            var bufferState = await processTask;
            this.ApplyBufferState(bufferState);
        }
    }
}