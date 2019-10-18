namespace Light.Json.Tokenization
{
    public interface IJsonToken
    {
        JsonTokenType Type { get; }
        int Length { get; }
        int Line { get; }
        int Position { get; }
        TokenCharacterInfo GetCharacterAt(int startIndex = 0);
    }
}