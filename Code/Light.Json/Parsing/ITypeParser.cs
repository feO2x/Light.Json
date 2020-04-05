﻿using Light.Json.Tokenization;

namespace Light.Json.Parsing
{
    public interface ITypeParser
    {
        object? ParseObject<TJsonTokenizer, TJsonToken>(in DeserializationContext context, ref TJsonTokenizer tokenizer)
            where TJsonTokenizer : struct, IJsonTokenizer<TJsonToken>
            where TJsonToken : struct, IJsonToken;
    }

    public interface ITypeParser<out T> : ITypeParser
    {
        T Parse<TJsonTokenizer, TJsonToken>(in DeserializationContext context, ref TJsonTokenizer tokenizer)
            where TJsonTokenizer : struct, IJsonTokenizer<TJsonToken>
            where TJsonToken : struct, IJsonToken;
    }
}