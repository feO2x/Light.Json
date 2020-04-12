namespace Light.Json.Deserialization.Tokenization
{
    public interface IJsonTokenizer<out TToken> where TToken : IJsonToken
    {
        TToken GetNextToken();
        string ReadString();
        int ReadInt32();
        void ReadBeginOfObject();
        void ReadEndOfObject();
        void ReadNameValueSeparator();
        TToken ReadNameToken();
    }
}