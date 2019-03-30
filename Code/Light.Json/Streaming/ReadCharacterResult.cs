namespace Light.Json.Streaming
{
    public readonly struct ReadCharacterResult
    {
        public readonly bool WasReadSuccessful;
        public readonly char Character;

        private ReadCharacterResult(bool wasReadSuccessful, char character)
        {
            WasReadSuccessful = wasReadSuccessful;
            Character = character;
        }

        public static implicit operator ReadCharacterResult(char character) =>
            new ReadCharacterResult(true, character);

        public static ReadCharacterResult NoCharacterAvailable => new ReadCharacterResult(false, default);
    }
}