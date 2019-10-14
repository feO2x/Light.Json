using System;

namespace Light.Json.Tokenization
{
    public interface IJsonToken
    {
        JsonTokenType Type { get; }
        int Length { get; }
        TokenCharacterInfo GetCharacterAt(int startIndex = 0);
    }
}