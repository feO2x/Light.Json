namespace Light.Json.Tokenization
{
    public interface IJsonToken
    {
        JsonTokenType Type { get; }
        int Length { get; }
        int Line { get; }
        int Position { get; }
        TokenCharacterInfo GetCharacterAt(int startIndex = 0);
        string ParseJsonStringToDotnetString();
    }

    public static class JsonTokenExtensions
    {
        public static void MustBeOfType<TJsonToken>(this TJsonToken token, JsonTokenType expectedType)
            where TJsonToken : struct, IJsonToken
        {
            if (token.Type == expectedType)
                return;

            throw new DeserializationException($"Expected token {token.ToString()} at line {token.Line} position {token.Position} to have type {expectedType}, but it actually has type {token.Type}.");
        }
    }
}