using System;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.Json.Parsing;
using Light.Json.Tokenization.Utf16;

namespace Light.Json
{
    public sealed class JsonDeserializer
    {
        private readonly Dictionary<Type, Utf16Parser> _utf16Parsers;

        public JsonDeserializer(Dictionary<Type, Utf16Parser> utf16Parsers)
        {
            _utf16Parsers = utf16Parsers.MustNotBeNull(nameof(utf16Parsers));
        }

        public T Deserialize<T>(ReadOnlySpan<char> json)
        {
            if (!_utf16Parsers.TryGetValue(typeof(T), out var parser))
                throw new DeserializationException($"There is no parser for type \"{typeof(T)}\".");
            if (!(parser is Utf16Parser<T> genericParser))
                throw new DeserializationException($"The parser for \"{parser.TargetType}\" is present but does not derive from {typeof(Utf16Parser<T>)}. It cannot be used to deserialize the JSON document.");

            var context = new Utf16Context(new JsonUtf16Tokenizer(json));
            return genericParser.ParseGeneric(context);
        }
    }
}