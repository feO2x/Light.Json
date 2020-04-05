using Light.Json.FrameworkExtensions;
using Light.Json.Tokenization;

namespace Light.Json.Parsing
{
    public abstract class BaseTypeParser<T> : ITypeParser<T>
    {
        protected BaseTypeParser(string? key = null) => TypeKey = new TypeKey(typeof(T), key);

        protected BaseTypeParser(TypeKey typeKey) => TypeKey = typeKey;

        public TypeKey TypeKey { get; }

        public abstract T Parse<TJsonTokenizer, TJsonToken>(in DeserializationContext context, ref TJsonTokenizer tokenizer)
            where TJsonTokenizer : struct, IJsonTokenizer<TJsonToken>
            where TJsonToken : struct, IJsonToken;

        public object? ParseObject<TJsonTokenizer, TJsonToken>(in DeserializationContext context, ref TJsonTokenizer tokenizer)
            where TJsonTokenizer : struct, IJsonTokenizer<TJsonToken>
            where TJsonToken : struct, IJsonToken
        {
            return Parse<TJsonTokenizer, TJsonToken>(in context, ref tokenizer);
        }
    }
}