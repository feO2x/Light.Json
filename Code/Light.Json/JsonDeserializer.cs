using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Light.Json.Deserialization.Parsing;
using Light.Json.Deserialization.Tokenization;
using Light.Json.Deserialization.Tokenization.Utf16;
using Light.Json.Deserialization.Tokenization.Utf8;
using Light.Json.FrameworkExtensions;

namespace Light.Json
{
    public sealed class JsonDeserializer
    {
        private readonly ITypeParserProvider _typeParserProvider;

        public JsonDeserializer(ITypeParserProvider? typeParserProvider = null) =>
            _typeParserProvider = typeParserProvider ?? new ImmutableTypeParserProvider(new Dictionary<TypeKey, ITypeParser>()); // TODO: must be replaced with Roslyn implementation

        public T Deserialize<T>(string utf16Json, string? parserKey = null) => Deserialize<T>(utf16Json.AsMemory(), parserKey);

        public T Deserialize<T>(ReadOnlyMemory<char> utf16Json, string? parserKey = null)
        {
            var tokenizer = new JsonUtf16Tokenizer(utf16Json);
            var context = new DeserializationContext(this);
            return Deserialize<T, JsonUtf16Tokenizer, JsonUtf16Token>(ref context, ref tokenizer, parserKey);
        }

        public T Deserialize<T>(byte[] utf8Json, string? parserKey = null) => Deserialize<T>(utf8Json.AsMemory(), parserKey);

        public T Deserialize<T>(ReadOnlyMemory<byte> utf8Json, string? parserKey = null)
        {
            var tokenizer = new JsonUtf8Tokenizer(utf8Json);
            var context = new DeserializationContext(this);
            return Deserialize<T, JsonUtf8Tokenizer, JsonUtf8Token>(ref context, ref tokenizer, parserKey);
        }

        public TResult Deserialize<TResult, TJsonTokenizer, TJsonToken>(ref DeserializationContext context, ref TJsonTokenizer tokenizer, string? typeParserKey)
            where TJsonTokenizer : struct, IJsonTokenizer<TJsonToken>
            where TJsonToken : struct, IJsonToken
        {
            if (typeof(TResult) == typeof(string))
            {
                var @string = tokenizer.ReadString();
                return Unsafe.As<string, TResult>(ref @string);
            }

            if (typeof(TResult) == typeof(int))
            {
                var int32 = tokenizer.ReadInt32();
                return Unsafe.As<int, TResult>(ref int32);
            }

            var targetType = typeof(TResult);
            if (!_typeParserProvider.TryGetTypeParser<TResult>(new TypeKey(typeof(TResult), typeParserKey), out var typeParser))
                throw new DeserializationException($"The type \"${targetType}\" cannot be deserialized because there is no type parser registered with the deserializer.");

            return typeParser.Parse<TJsonTokenizer, TJsonToken>(context, ref tokenizer);
        }
    }
}