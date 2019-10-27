namespace Light.Json.Tokenization
{
    public interface IJsonTokenizer<out TToken> where TToken : IJsonToken
    {
        TToken GetNextToken();
    }
}