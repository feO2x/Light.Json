using System;
using System.Runtime.CompilerServices;
using Light.Json.Parsing;
using Light.Json.Tokenization;
using Light.Json.Tokenization.Utf16;
using Light.Json.Tokenization.Utf8;

namespace Light.Json
{
    public sealed class JsonDeserializer
    {
        public T Deserialize<T>(ReadOnlyMemory<char> utf16Json)
        {
            var tokenizer = new JsonUtf16Tokenizer(utf16Json);
            var context = new JsonUtf16DeserializationContext(tokenizer, this);
            return Deserialize<T, JsonUtf16DeserializationContext, JsonUtf16Tokenizer, JsonUtf16Token>(ref context);
        }

        public T Deserialize<T>(ReadOnlyMemory<byte> utf8Json)
        {
            var tokenizer = new JsonUtf8Tokenizer(utf8Json);
            var context = new JsonUtf8DeserializationContext(tokenizer, this);
            return Deserialize<T, JsonUtf8DeserializationContext, JsonUtf8Tokenizer, JsonUtf8Token>(ref context);
        }

        public TResult Deserialize<TResult, TContext, TJsonTokenizer, TJsonToken>(ref TContext context)
            where TContext : struct, IDeserializationContext<TJsonTokenizer, TJsonToken>
            where TJsonTokenizer : struct, IJsonTokenizer<TJsonToken>
            where TJsonToken : struct, IJsonToken
        {
            if (typeof(TResult) == typeof(string))
            {
                var @string = context.Tokenizer.ReadString();
                return Unsafe.As<string, TResult>(ref @string);
            }
            if (typeof(TResult) == typeof(int))
            {
                var int32 = context.Tokenizer.ReadInt32();
                return Unsafe.As<int, TResult>(ref int32);
            }

            throw new NotImplementedException();
        }
    }
}