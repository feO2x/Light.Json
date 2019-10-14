namespace Light.Json.Tokenization
{
    public readonly ref struct TokenCharacterInfo
    {
        public readonly int Utf16Utf16Character;
        public readonly int IndexOfNextCharacter;

        public TokenCharacterInfo(int utf16Character, int indexOfNextCharacter)
        {
            Utf16Utf16Character = utf16Character;
            IndexOfNextCharacter = indexOfNextCharacter;
        }
    }
}