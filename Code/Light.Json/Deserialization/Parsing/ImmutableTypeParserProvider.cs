using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Light.GuardClauses;
using Light.Json.FrameworkExtensions;

namespace Light.Json.Deserialization.Parsing
{
    public sealed class ImmutableTypeParserProvider : ITypeParserProvider
    {
        private readonly Dictionary<TypeKey, ITypeParser> _typeParsers;

        public ImmutableTypeParserProvider(Dictionary<TypeKey, ITypeParser> typeParsers) =>
            _typeParsers = typeParsers.MustNotBeNull(nameof(typeParsers));

        public bool TryGetTypeParser<T>(TypeKey typeKey, [NotNullWhen(true)] out ITypeParser<T>? typeParser)
        {
            if (_typeParsers.TryGetValue(typeKey, out var internalTypeParser) && internalTypeParser is ITypeParser<T> genericTypeParser)
            {
                typeParser = genericTypeParser;
                return true;
            }

            typeParser = default;
            return false;
        }
    }
}